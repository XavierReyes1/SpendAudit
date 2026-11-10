namespace SpendAudit.Application.Common.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Empleado = "Empleado";
    public const string Auditor = "Auditor";
}

public static class LimitesArchivo
{
    public const long TamanoMaximoBytes = 10 * 1024 * 1024; // 10 MB
    public static readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };
}

public static class ClaimsNombres
{
    public const string EmpresaId = "empresaId";
    public const string UsuarioId = "sub";
    public const string Rol = "role";
}

/// <summary>Honduras no usa horario de verano: UTC-6 todo el año.</summary>
public static class HoraLocal
{
    public static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));
}
