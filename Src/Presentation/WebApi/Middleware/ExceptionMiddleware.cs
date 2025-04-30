using BancaCore.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BancaCore.WebApi.Common
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió una excepción.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            object problem = new ProblemDetails
            {
                Title = "Error interno del servidor",
                Detail = "Ocurrió un error inesperado. Por favor, contacte al administrador.",
                Status = StatusCodes.Status500InternalServerError
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problem = new HttpValidationProblemDetails(validationEx.Failures)
                    {
                        Title = "Error de validación",
                        Detail = "Uno o más errores de validación han ocurrido.",
                        Status = StatusCodes.Status400BadRequest
                    };
                    break;

                case BadRequestException badRequestEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problem = new ProblemDetails
                    {
                        Title = "Solicitud incorrecta",
                        Detail = badRequestEx.Message,
                        Status = StatusCodes.Status400BadRequest
                    };
                    break;

                case NotFoundException notFoundEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    problem = new ProblemDetails
                    {
                        Title = "Recurso no encontrado",
                        Detail = notFoundEx.Message,
                        Status = StatusCodes.Status404NotFound
                    };
                    break;
            }

            var json = JsonConvert.SerializeObject(problem);
            return context.Response.WriteAsync(json);
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
