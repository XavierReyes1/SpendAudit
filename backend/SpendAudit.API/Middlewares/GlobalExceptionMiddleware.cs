using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SpendAudit.Application.Common.Exceptions;

namespace SpendAudit.API.Middlewares;

/// <summary>
/// Punto único donde las excepciones de las capas internas se traducen a respuestas HTTP.
/// Los servicios solo lanzan excepciones de negocio; aquí se decide el código y el cuerpo.
///   400 → lista [{ propertyName, errorMessage }]  (FluentValidation, misma forma que la Clase 8)
///   401/403/404/409 → { mensaje }
///   500 → { mensaje } genérico (el detalle solo va al log)
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión: no hay a quién responder.
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var (status, cuerpo) = Traducir(ex);

            if (status >= 500)
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(cuerpo);
        }
    }

    private static (int Status, object Cuerpo) Traducir(Exception ex) => ex switch
    {
        ValidationException v => (StatusCodes.Status400BadRequest,
            v.Errors.Select(e => new { propertyName = e.PropertyName, errorMessage = e.ErrorMessage }).ToList()),

        BadHttpRequestException b => (StatusCodes.Status400BadRequest,
            new List<object> { new { propertyName = "", errorMessage = b.Message } }),

        UnauthorizedException u => (StatusCodes.Status401Unauthorized, new { mensaje = u.Message }),
        ForbiddenException f => (StatusCodes.Status403Forbidden, new { mensaje = f.Message }),
        NotFoundException n => (StatusCodes.Status404NotFound, new { mensaje = n.Message }),
        ConflictException c => (StatusCodes.Status409Conflict, new { mensaje = c.Message }),

        // Red de seguridad ante carreras: dos peticiones simultáneas que violan un índice único.
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
            (StatusCodes.Status409Conflict, new { mensaje = "Ya existe un registro con esos datos." }),

        _ => (StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor." })
    };
}