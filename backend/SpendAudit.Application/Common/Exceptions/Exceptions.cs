namespace SpendAudit.Application.Common.Exceptions;

/// <summary>El recurso no existe (o pertenece a otra empresa) → HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string mensaje) : base(mensaje) { }
}

/// <summary>Conflicto con el estado actual (duplicados, editar algo ya liquidado) → HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string mensaje) : base(mensaje) { }
}

/// <summary>Credenciales inválidas o sesión ausente → HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string mensaje) : base(mensaje) { }
}

/// <summary>Rol insuficiente → HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string mensaje) : base(mensaje) { }
}
