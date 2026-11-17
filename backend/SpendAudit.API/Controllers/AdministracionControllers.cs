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
public class PuestosController(IUsuariosService usuariosService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Auditor}")]
    public async Task<ActionResult<List<PuestoResponseDto>>> Listar(CancellationToken ct) =>
        Ok(await usuariosService.ListarPuestosAsync(ct));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PuestoResponseDto>> Crear(CreatePuestoRequestDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await usuariosService.CrearPuestoAsync(dto, ct));
}

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/v1/usuarios")]
public class UsuariosController(IUsuariosService usuariosService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct) =>
        Ok(await usuariosService.ListarUsuariosAsync(ct));

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(CreateUsuarioRequestDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await usuariosService.CrearUsuarioAsync(dto, ct));
}

[ApiController]
[Authorize]
[Route("api/v1/politicas")]
public class PoliticasController(IPoliticasService politicasService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Auditor}")]
    public async Task<ActionResult<PoliticaResponseDto>> ObtenerActiva(CancellationToken ct) =>
        Ok(await politicasService.ObtenerActivaAsync(ct));

    [HttpPut]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PoliticaResponseDto>> Actualizar(UpdatePoliticaRequestDto dto, CancellationToken ct) =>
        Ok(await politicasService.ActualizarAsync(dto, ct));
}
