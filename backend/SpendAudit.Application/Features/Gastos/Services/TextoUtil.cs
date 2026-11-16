using System.Globalization;
using System.Text.RegularExpressions;

namespace SpendAudit.Application.Features.Gastos.Services;

public static partial class TextoUtil
{
    public static string NormalizarTexto(string? textoCrudo)
    {
        if (string.IsNullOrWhiteSpace(textoCrudo))
            return string.Empty;

        return Regex.Replace(textoCrudo, @"\s+", " ").Trim();
    }

    public static string? ExtraerRtn(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var match = RtnRegex().Match(texto);
        if (!match.Success) return null;
        var soloDigitos = Regex.Replace(match.Value, @"\D", "");
        return soloDigitos.Length == 14 ? soloDigitos : null;
    }

    public static string? ExtraerCai(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var match = CaiRegex().Match(texto);
        return match.Success ? match.Value.ToUpperInvariant() : null;
    }

    public static string? ExtraerNumeroFactura(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var match = FacturaRegex().Match(texto);
        return match.Success ? match.Value : null;
    }

    public static DateOnly? ExtraerFechaEmision(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var match = FechaRegex().Match(texto);
        if (!match.Success) return null;

        string[] formatos = ["dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "d/M/yyyy", "d-M-yyyy"];
        return DateOnly.TryParseExact(match.Value, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha) 
            ? fecha 
            : null;
    }

    public static decimal? ExtraerMontoTotal(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var match = TotalRegex().Match(texto);
        return match.Success ? ParsearMontoHnl(match.Groups["monto"].Value) : null;
    }

    public static decimal? ExtraerIsv(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var match = IsvRegex().Match(texto);
        return match.Success ? ParsearMontoHnl(match.Groups["monto"].Value) : null;
    }

    private static decimal? ParsearMontoHnl(string valorCrudo)
    {
        var limpio = valorCrudo.Replace("L", "", StringComparison.OrdinalIgnoreCase)
                               .Replace(",", "")
                               .Trim();
        return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto) ? monto : null;
    }

    [GeneratedRegex(@"\b\d{4}-?\d{4}-?\d{6}\b")]
    private static partial Regex RtnRegex();

    [GeneratedRegex(@"\b[A-Fa-f0-9]{6}-[A-Fa-f0-9]{6}-[A-Fa-f0-9]{6}-[A-Fa-f0-9]{6}-[A-Fa-f0-9]{6}-[A-Fa-f0-9]{2}\b")]
    private static partial Regex CaiRegex();

    [GeneratedRegex(@"\b\d{3}-\d{3}-\d{2}-\d{8}\b")]
    private static partial Regex FacturaRegex();

    [GeneratedRegex(@"\b(\d{2}[/-]\d{2}[/-]\d{4}|\d{4}-\d{2}-\d{2})\b")]
    private static partial Regex FechaRegex();

    [GeneratedRegex(@"(?:TOTAL\s*(?:A\s*PAGAR|GENERAL|NETO)?)\s*:?\s*L?\.?\s*(?<monto>\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:\.\d{2}))", RegexOptions.IgnoreCase)]
    private static partial Regex TotalRegex();

    [GeneratedRegex(@"(?:I\.?S\.?V\.?\s*(?:15%|18%)?)\s*:?\s*L?\.?\s*(?<monto>\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:\.\d{2}))", RegexOptions.IgnoreCase)]
    private static partial Regex IsvRegex();
}