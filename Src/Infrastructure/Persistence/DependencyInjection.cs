using BancaCore.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BancaCore.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BancaDbContext>(options =>
                options
                    .UseSqlite(configuration.GetConnectionString("Database"))
                    .EnableSensitiveDataLogging() // Habilitar para ver los parámetros de la consulta
                    .EnableDetailedErrors()       // Habilitar para obtener errores detallados
                    .LogTo(Console.WriteLine, LogLevel.Information) // Redirigir los logs a la consola    
            );
            services.AddTransient<IFireForgetCommandHandler, FireForgetCommandHandler>();
            services.AddScoped<IBancaDbContext>(provider => provider.GetService<BancaDbContext>());
            
            // Register the database seeder
            services.AddScoped<DatabaseSeeder>();
            
            return services;
        }
    }
}