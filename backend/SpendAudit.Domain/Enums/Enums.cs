namespace SpendAudit.Domain.Enums;

public enum RolUsuario { Admin, Empleado, Auditor }

public enum EstadoSuscripcion { Activa, Suspendida }

public enum TipoArchivo { Pdf, Imagen }

/// <summary>Estado del flujo del comprobante.</summary>
public enum EstadoComprobante { Borrador, Liquidado }

/// <summary>Semáforo de auditoría. El orden numérico es la severidad.</summary>
public enum ResultadoAuditoria { Verde = 0, Amarillo = 1, Rojo = 2 }

public enum ReglaAuditoria
{
    FacturaDuplicada,
    TerminoRestringido,
    FacturaVencida,
    TopeDiarioExcedido
}

public enum SeveridadHallazgo { Amarillo = 1, Rojo = 2 }
