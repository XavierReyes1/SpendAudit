using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendAudit.Application.Common.Models;
using SpendAudit.Application.Features.Politicas.DTOs;
using SpendAudit.Application.Features.Politicas.Services;
using SpendAudit.Application.Features.Usuarios.DTOs;
using SpendAudit.Application.Features.Usuarios.Services;

namespace SpendAudit.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/puestos")]
public class PuestosController : ControllerBase
{
    private readonly IUsuariosService _service;
    public PuestosController(IUsuariosService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Auditor}")]
    public async Task<ActionResult<List<PuestoResponseDto>>> Listar(CancellationToken ct) =>
        Ok(await _service.ListarPuestosAsync(ct));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Crear(CreatePuestoRequestDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _service.CrearPuestoAsync(dto, ct));
}

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/v1/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _service;
    public UsuariosController(IUsuariosService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct) =>
        Ok(await _service.ListarUsuariosAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Crear(CreateUsuarioRequestDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _service.CrearUsuarioAsync(dto, ct));
}

[ApiController]
[Authorize]
[Route("api/v1/politicas")]
public class PoliticasController : ControllerBase
{
    private readonly IPoliticasService _service;
    public PoliticasController(IPoliticasService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Auditor}")]
    public async Task<ActionResult<PoliticaResponseDto>> Obtener(CancellationToken ct) =>
        Ok(await _service.ObtenerAsync(ct));

    [HttpPut]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PoliticaResponseDto>> Actualizar(UpdatePoliticaRequestDto dto, CancellationToken ct) =>
        Ok(await _service.ActualizarAsync(dto, ct));
}