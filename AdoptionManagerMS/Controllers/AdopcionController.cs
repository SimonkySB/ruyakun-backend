using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace AdoptionManagerMS.Controllers;


[ApiController]
[Route("adopciones")]
public class AdopcionController(AdopcionService adopcionService, UsuarioService usuarioService) : ControllerBase
{

    [HttpGet]
    [Authorize]
    public async Task<ActionResult> List([FromQuery] AdopcionQuery query)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        if (User.ISSuperAdmin())
        {
            var res = await adopcionService.List(query);
            return Ok(res);
        }
        
        if (User.IsUser())
        {
            query.usuarioId = usuario.usuarioId;
            var res = await adopcionService.List(query);
            return Ok(res);
        }
        
        if (User.IsAdmin() || User.IsColaborator())
        {
            var res = await adopcionService.ListByAdminColaborador(usuario.usuarioId);
            return Ok(res);
        }

        return Ok(new List<string>());
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult> GetById(int id)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        

        if (User.ISSuperAdmin())
        {
            var res = await adopcionService.GetById(id);
            if (res == null)
            {
                return NotFound("Adopcion no encontrada");
            }
            return Ok(res);
        }  
        
        if (User.IsUser())
        {
            var res = await adopcionService.GetByIdAndUser(id, usuario.usuarioId);
            if (res == null)
            {
                return NotFound("Adopcion no encontrada");
            }
            return Ok(res);
        }
        
        if (User.IsAdmin() || User.IsColaborator())
        {
            var res = await adopcionService.GetByIdAndAdminColaborador(id, usuario.usuarioId);
            if (res == null)
            {
                return NotFound("Adopcion no encontrada");
            }
            return Ok(res);
        }
        
        return NotFound("Adopcion no encontrada");
    }

    [HttpPost("solicitar")]
    public async Task<ActionResult> Solicitar(AdopcionSolicitarRequest request)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        request.usuarioId = usuario.usuarioId;
        
        var res = await adopcionService.Solicitar(request);
        return Ok(res);
    }

    [HttpPost("{id}/aprobar")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> Aprobar(int id)
    {
        if (!User.ISSuperAdmin())
        {
            await VerificaAdopcion(id);
        }
        
        var res = await adopcionService.Aprobar(id);
        return Ok(res);
    }
  
    [HttpPost("{id}/rechazar")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> Rechazar(int id)
    {
        if (!User.ISSuperAdmin())
        {
            await VerificaAdopcion(id);
        }
        var res = await adopcionService.Rechazar(id);
        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<ActionResult> Delete(int id)
    {
        if (!User.ISSuperAdmin())
        {
            await VerificaAdopcion(id);
        }
        
        await adopcionService.Eliminar(id);
        return NoContent();
    }
  

    [HttpGet("estados")]
    public async Task<ActionResult> ListEstados()
    {
        var res = await adopcionService.ListAdopcionEstados();
        return Ok(res);
    }


    private async Task VerificaAdopcion(int id)
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        await usuarioService.VerificaAdopcionUsuario(id, usuario.usuarioId);
    }
}