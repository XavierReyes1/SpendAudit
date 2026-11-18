using SpendAudit.Domain.Enums;

namespace SpendAudit.Application.Features.Gastos.Services;

/// <summary>
/// Verifica los primeros bytes del archivo (magic numbers) para que nadie suba un .exe renombrado a .pdf.
/// </summary>
public static class FirmaArchivo
{
    public static TipoArchivo? Detectar(ReadOnlySpan<byte> inicio, string extension)
    {
        extension = extension.ToLowerInvariant();

        if (extension == ".pdf" && inicio.Length >= 5 &&
            inicio[0] == 0x25 && inicio[1] == 0x50 && inicio[2] == 0x44 && inicio[3] == 0x46 && inicio[4] == 0x2D)
            return TipoArchivo.Pdf; // %PDF-

        if ((extension is ".jpg" or ".jpeg") && inicio.Length >= 3 &&
            inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF)
            return TipoArchivo.Imagen;

        if (extension == ".png" && inicio.Length >= 8 &&
            inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47 &&
            inicio[4] == 0x0D && inicio[5] == 0x0A && inicio[6] == 0x1A && inicio[7] == 0x0A)
            return TipoArchivo.Imagen;

        return null;
    }

    public static string ContentType(string nombreArchivo) =>
        Path.GetExtension(nombreArchivo).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
}
