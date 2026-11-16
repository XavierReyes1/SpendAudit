using MediatR;
using SpendAudit.Application.Interfaces;
using SpendAudit.Domain.Entities;
using SpendAudit.Domain.Enums;

using SpendAudit.Application.Features.Gastos.Services; // Para encontrar ParserComprobante

namespace SpendAudit.Application.Features.Gastos.Commands.CrearBorrador;

public class CrearGastoBorradorCommandHandler : IRequestHandler<CrearGastoBorradorCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExtractorTextoService _extractorTextoService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentTenantService _currentTenantService;

    public CrearGastoBorradorCommandHandler(
        IUnitOfWork unitOfWork,
        IExtractorTextoService extractorTextoService,
        IFileStorageService fileStorageService,
        ICurrentTenantService currentTenantService)
    {
        _unitOfWork = unitOfWork;
        _extractorTextoService = extractorTextoService;
        _fileStorageService = fileStorageService;
        _currentTenantService = currentTenantService;
    }

    public async Task<Guid> Handle(CrearGastoBorradorCommand request, CancellationToken ct)
    {
        var empresaId = _currentTenantService.EmpresaId ?? throw new UnauthorizedAccessException("Empresa no identificada.");
        var usuarioId = _currentTenantService.UsuarioId ?? throw new UnauthorizedAccessException("Usuario no identificado.");

        string? archivoClave = null;
        DatosComprobanteExtraidos? datosParser = null;

        // 1. Si el usuario subió un archivo, lo guardamos y lo procesamos con el Parser
        if (request.Archivo != null && request.Archivo.Length > 0)
        {
            var extension = Path.GetExtension(request.Archivo.FileName);
            
            // Guardar en almacenamiento seguro
            using var streamParaGuardar = request.Archivo.OpenReadStream();
            archivoClave = await _fileStorageService.GuardarAsync(streamParaGuardar, extension, empresaId, usuarioId, ct);

            // Procesar el archivo con tu ParserComprobante
            using var streamParaParsear = request.Archivo.OpenReadStream();
            var parser = new ParserComprobante();
            datosParser = parser.ProcesarArchivo(streamParaParsear, extension);
        }

        // 2. REGLA DE ORO: Prioridad manual sobre lo extraído automáticamente por el Parser
        decimal totalFinal = request.TotalManual ?? datosParser?.MontoTotal ?? 0;
        decimal isvFinal = request.IsvManual ?? datosParser?.Isv ?? 0;
        string? rtnFinal = request.RtnManual ?? datosParser?.ProveedorRtn;
        string? caiFinal = request.CaiManual ?? datosParser?.Cai;
        string? facturaFinal = request.NumeroFacturaManual ?? datosParser?.NumeroFactura;
        DateTime? fechaFinal = request.FechaManual ?? (datosParser?.FechaEmision.HasValue == true ? datosParser.FechaEmision.Value.ToDateTime(TimeOnly.MinValue) : DateTime.UtcNow);

        var comprobante = new Comprobante
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            MontoTotal = totalFinal,
            ISV = isvFinal,
            ProveedorRTN = rtnFinal,
            CAI = caiFinal,
            NumeroFactura = facturaFinal,
            FechaEmision = fechaFinal.HasValue ? DateOnly.FromDateTime(fechaFinal.Value) : null,
            UrlArchivo = archivoClave ?? string.Empty,
            NombreArchivoOriginal = request.Archivo?.FileName ?? string.Empty,
            TipoArchivo = TipoArchivo.Pdf,
            Estado = EstadoComprobante.Borrador
        };

        // 4. Persistir en la base de datos a través de UnitOfWork
        // (Si tu UnitOfWork expone un repositorio de comprobantes, asegúrate de agregarlo, por ejemplo:
        // _unitOfWork.Comprobantes.Add(comprobante);)
        
        await _unitOfWork.SaveChangesAsync(ct);

        return comprobante.Id;
    }
}