using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendAudit.Application.Features.Gastos.Commands.CrearBorrador;

namespace SpendAudit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/gastos")]
public class GastosController : ControllerBase
{
    private readonly IMediator _mediator;

    public GastosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("borrador")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CrearBorrador([FromForm] CrearGastoBorradorCommand command, CancellationToken ct)
    {
        var gastoId = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(CrearBorrador), new { id = gastoId }, new { Id = gastoId });
    }
}