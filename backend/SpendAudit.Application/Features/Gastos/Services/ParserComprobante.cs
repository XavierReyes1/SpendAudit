using UglyToad.PdfPig;

namespace SpendAudit.Application.Features.Gastos.Services;

public record DatosComprobanteExtraidos(
    string TextoCrudo,
    string? ProveedorRtn,
    string? Cai,
    string? NumeroFactura,
    DateOnly? FechaEmision,
    decimal? Isv,
    decimal? MontoTotal
);

public class ParserComprobante
{
    public DatosComprobanteExtraidos ProcesarArchivo(Stream archivoStream, string extension)
    {
        if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return new DatosComprobanteExtraidos(string.Empty, null, null, null, null, null, null);
        }

        var textoExtraido = ExtraerTextoDePdf(archivoStream);
        var textoNormalizado = TextoUtil.NormalizarTexto(textoExtraido);

        return new DatosComprobanteExtraidos(
            TextoCrudo: textoExtraido,
            ProveedorRtn: TextoUtil.ExtraerRtn(textoNormalizado),
            Cai: TextoUtil.ExtraerCai(textoNormalizado),
            NumeroFactura: TextoUtil.ExtraerNumeroFactura(textoNormalizado),
            FechaEmision: TextoUtil.ExtraerFechaEmision(textoNormalizado),
            Isv: TextoUtil.ExtraerIsv(textoNormalizado),
            MontoTotal: TextoUtil.ExtraerMontoTotal(textoNormalizado)
        );
    }

    private static string ExtraerTextoDePdf(Stream pdfStream)
    {
        if (pdfStream.CanSeek)
            pdfStream.Position = 0;

        using var documento = PdfDocument.Open(pdfStream);
        var paginas = documento.GetPages().Select(p => p.Text);
        return string.Join(Environment.NewLine, paginas);
    }
}