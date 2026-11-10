namespace SpendAudit.Application.Features.Politicas.DTOs;

public class TopePuestoDto
{
    public Guid PuestoId { get; set; }
    public string? Puesto { get; set; }
    public decimal TopeDiarioHNL { get; set; }
}

public class PoliticaResponseDto
{
    public int DiasVigenciaMaxima { get; set; }
    public List<TopePuestoDto> Topes { get; set; } = new();
    public List<string> TerminosRestringidos { get; set; } = new();
}

public class UpdatePoliticaRequestDto
{
    public int DiasVigenciaMaxima { get; set; }
    public List<TopePuestoDto> Topes { get; set; } = new();
    public List<string> TerminosRestringidos { get; set; } = new();
}
