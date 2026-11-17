using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SpendAudit.Application.Common.Exceptions;

namespace SpendAudit.API.Middlewares;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await ResponderAsync(context, ex);
        }
    }

    private async Task ResponderAsync(HttpContext context, Exception ex)
    {
        (int status, object cuerpo) respuesta = ex switch
        {
            ValidationException validacion => (StatusCodes.Status400BadRequest,
                validacion.Errors.Select(error => new { error.PropertyName, error.ErrorMessage }).ToList()),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, new { mensaje = ex.Message }),
            ForbiddenException => (StatusCodes.Status403Forbidden, new { mensaje = ex.Message }),
            NotFoundException => (StatusCodes.Status404NotFound, new { mensaje = ex.Message }),
            ConflictException => (StatusCodes.Status409Conflict, new { mensaje = ex.Message }),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                (StatusCodes.Status409Conflict, new { mensaje = "El registro ya existe." }),
            _ => (StatusCodes.Status500InternalServerError, new { mensaje = "Error interno" })
        };

        if (respuesta.status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

        context.Response.Clear();
        context.Response.StatusCode = respuesta.status;
        await context.Response.WriteAsJsonAsync(respuesta.cuerpo, respuesta.cuerpo.GetType());
    }
}
