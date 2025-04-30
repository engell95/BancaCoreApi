using BancaCore.Application;
using BancaCore.Common.Interfaces;
using BancaCore.Persistence;
using BancaCore.WebApi.Common;
using BancaCore.WebApi.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System;
using System.IO;

// Iniciar NLog con la configuración desde appsettings.json
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Inicia Aplicacion");

try
{
    // Crear el constructor de la aplicación
    var builder = WebApplication.CreateBuilder(args);

    // Configurar NLog como el proveedor de logging predeterminado
    builder.Logging.ClearProviders();  // Elimina los proveedores de logging predeterminados
    builder.Host.UseNLog();  // Usa NLog para el manejo de logs

    // Registrar la configuración de bancaCore en el contenedor de servicios
    builder.Services.Configure<BancaCoreConfiguration>(builder.Configuration.GetSection("BancaCoreConfiguration"));

    // Obtener una instancia de la configuración de bancaCore directamente desde builder.Configuration
    var BancaCoreConfig = builder.Configuration.GetSection("BancaCoreConfiguration").Get<BancaCoreConfiguration>();

    // Agregar servicios de controladores y explorador de API al contenedor
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true);

    // Configurar CORS (Cross-Origin Resource Sharing) para permitir o restringir orígenes
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
        builder =>
        {
            if (BancaCoreConfig.CorsAllowAnyOrigin)
            {
                builder.AllowAnyOrigin();
            }
            else
            {
                builder.WithOrigins(BancaCoreConfig.CorsAllowOrigins);
            }

            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
        });
    });

    // Configurar Swagger utilizando la configuración obtenida
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(BancaCoreConfig.ApiVersion,
            new OpenApiInfo
            {
                Title = BancaCoreConfig.ApiName,
                Version = BancaCoreConfig.ApiVersion,
                Description = BancaCoreConfig.ApiDescription,
                Contact = new OpenApiContact
                {
                    Name = BancaCoreConfig.OpenApiContact.Name,
                    Url = BancaCoreConfig.OpenApiContact.Url,
                    Email = BancaCoreConfig.OpenApiContact.Email
                }
            });
    });

    // Agregar servicios de persistencia e infraestructura al contenedor, pasando la configuración de la aplicación
    builder.Services.AddPersistence(builder.Configuration);
    //builder.Services.AddInfrastructure(builder.Configuration);

    // Agregar capa de aplicación
    builder.Services.AddApplication();

    // Registro del Health Check para DbContext
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<BancaDbContext>();

    builder.Services.AddHttpContextAccessor();

    // Agregar servicios MVC con soporte para JSON a través de Newtonsoft
    builder.Services.AddControllersWithViews().AddNewtonsoftJson();

    // Registrar FluentValidation para ASP.NET Core
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    // Registrar todos los validadores desde el ensamblado que contiene IJupemaDbContext
    builder.Services.AddValidatorsFromAssemblyContaining<IBancaDbContext>();

    // Customise default API behaviour
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });


    builder.Services.AddHttpContextAccessor();

    // Construir la aplicación
    var app = builder.Build();

    // Crear la base de datos si no existe
    Directory.CreateDirectory("Data"); // Crea la carpeta si no existe

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BancaDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        try
        {
            // Ensure database is created and seed with test data if empty
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while initializing the database.");
        }
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // Configuración del pipeline de manejo de solicitudes HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // Configurar Swagger UI con la información de configuración de la API
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"{BancaCoreConfig.ApiBaseUrl}/swagger/{BancaCoreConfig.ApiVersion}/swagger.json", BancaCoreConfig.ApiName);
        c.OAuthClientId(BancaCoreConfig.OidcSwaggerUIClientId);
        c.OAuthAppName(BancaCoreConfig.ApiName);
        c.OAuthUsePkce();
    });

    // Configuración de manejo global de excepciones
    app.UseCustomExceptionHandler();
   // app.UseHealthChecks("/health");

    // Otros middleware esenciales
    app.UseCors();  // Habilitar CORS para la API
    app.UseHttpsRedirection();  // Redirige automáticamente las solicitudes HTTP a HTTPS

    // Use Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Mapear los controladores
    app.MapControllers();

    // Ejecutar la aplicación
    app.Run();
}
catch (Exception ex)
{
    // Captura cualquier excepción no manejada durante el arranque de la aplicación
    logger.Error(ex, "La aplicación se detuvo debido a una excepción no manejada.");
    throw;
}
finally
{
    // Asegura el cierre correcto de NLog al apagar la aplicación
    NLog.LogManager.Shutdown();
}
