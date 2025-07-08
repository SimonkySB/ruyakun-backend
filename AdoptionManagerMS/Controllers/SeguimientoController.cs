using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace AdoptionManagerMS.Controllers;

[ApiController]
[Route("seguimientos")]
public class SeguimientoController(ISeguimientoService seguimientoService, IUsuarioService usuarioService) : ControllerBase
{

    [HttpGet]
    [Authorize]
    public async Task<ActionResult> GetSeguimientos([FromQuery] SeguimientoQuery query)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        if (User.ISSuperAdmin())
        {
            var res = await seguimientoService.List(query);
            return Ok(res);
        }

        if (User.IsUser())
        {
            query.usuarioId = usuario.usuarioId;
            var res = await seguimientoService.List(query);
            return Ok(res);
        }

        if (User.IsAdmin() || User.IsColaborator())
        {
            var res = await seguimientoService.ListByAdminColaborador(usuario.usuarioId);
            return Ok(res);
        }
        return Ok(new List<string>());
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult> GetSeguimiento(int id)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        

        if (User.ISSuperAdmin())
        {
            var res = await seguimientoService.GetById(id);
            if (res == null)
            {
                return NotFound("Seguimiento no encontrado");
            }
            return Ok(res);
        }  
        
        if (User.IsUser())
        {
            var res = await seguimientoService.GetByIdAndUser(id, usuario.usuarioId);
            if (res == null)
            {
                return NotFound("Seguimiento no encontrado");
            }
            return Ok(res);
        }
        
        if (User.IsAdmin() || User.IsColaborator())
        {
            var res = await seguimientoService.GetByIdAndAdminColaborador(id, usuario.usuarioId);
            if (res == null)
            {
                return NotFound("Seguimiento no encontrado");
            }
            return Ok(res);
        }
        return NotFound("Seguimiento no encontrado");
    }

    [HttpPost]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> CrearSeguimiento(SeguimientoRequest request)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        if (!User.ISSuperAdmin())
        {
            await usuarioService.VerificaAdopcionUsuario(request.adopcionId, usuario.usuarioId);
        }
        
        var res = await seguimientoService.Crear(request);
        return Ok(res);
    }

    [HttpPut("{id}")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> EditarSeguimiento(int id, SeguimientoRequest request)
    {
        if (!User.ISSuperAdmin())
        {
            await VerificaSeguimiento(id);
        }
        
        var res = await seguimientoService.Editar(id, request);
        return Ok(res);
    }

   

    [HttpDelete("{id}")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> EliminarSeguimiento(int id)
    {
        if (!User.ISSuperAdmin())
        {
            await VerificaSeguimiento(id);
        }
        
        await seguimientoService.Eliminar(id);
        return NoContent();
    }
    
    [HttpPost("{id}/cerrar")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> CerrarSeguimiento(int id, SeguimientoCerrarRequest request)
    {
        if (!User.ISSuperAdmin())
        {
            await VerificaSeguimiento(id);
        }
        
        var res = await seguimientoService.Cerrar(id, request);
        return Ok(res);
    }


    [HttpGet("estados")]
    public async Task<ActionResult> GetEstados()
    {
        var res = await seguimientoService.GetEstados();
        return Ok(res);
    }
    
    [HttpGet("tipos")]
    public async Task<ActionResult> GetTipos()
    {
        var res = await seguimientoService.GetTipos();
        return Ok(res);
    }

    private async Task VerificaSeguimiento(int id)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        var seguimiento = await seguimientoService.GetById(id);
        if (seguimiento == null)
        {
            throw new AppException("Seguimiento no encontrado");
        }
        await usuarioService.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId);
    }
}