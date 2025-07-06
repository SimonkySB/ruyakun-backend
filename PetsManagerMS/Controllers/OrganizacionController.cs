using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;
using Shared;

namespace PetsManagerMS.Controllers;

[ApiController]
[Route("organizaciones")]
public class OrganizacionController(IOrganizacionService organizacionService, IUsuarioService usuarioService) : ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> List(int? usuarioId)
    {
        var res = await organizacionService.List(usuarioId);
        return Ok(res);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var res = await organizacionService.GetById(id);
        if (res == null)
        {
            return NotFound("Organizacion no encontrada");
        }
        return Ok(res);
    }

    [HttpPost]
    [Authorize(policy: Policies.SuperAdmin)]
    public async Task<IActionResult> Crear(OrganizacionRequest request)
    {
        
        var res = await organizacionService.Crear(request);
        
        return Ok(res);
    }
    
    [HttpPut("{id}")]
    [Authorize(policy: Policies.SuperAdmin)]
    public async Task<IActionResult> Editar(int id, OrganizacionRequest request)
    {
        var res = await organizacionService.Editar(id, request);
        return Ok(res);
    }
    
    [HttpDelete("{id}")]
    [Authorize(policy: Policies.SuperAdmin)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await organizacionService.Eliminar(id);
        return NoContent();
    }


    [HttpPost("{organizacionId}/usuarios/{usuarioId}")]
    [Authorize(policy: Policies.SuperAdmin)]
    public async Task<IActionResult> AgregarUsuario(int organizacionId, int usuarioId)
    {
        await organizacionService.AgregarUsuario(organizacionId, usuarioId);
        return Ok();
    }
    
    [HttpDelete("{organizacionId}/usuarios/{usuarioId}")]
    [Authorize(policy: Policies.SuperAdmin)]
    public async Task<IActionResult> QuitarUsuario(int organizacionId, int usuarioId)
    {
        await organizacionService.QuitarUsuario(organizacionId, usuarioId);
        return NoContent();
    }
    
    
    
    [HttpGet("me")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> ListOrganizacionesOwner()
    {
        var user = await usuarioService.VerificaUsuario(User.GetUsername());
        var res = await organizacionService.List(user.usuarioId);
        return Ok(res);
    }
    
    
    [HttpPut("{id}/me")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> EditarOrganizacionOwner(int id, OrganizacionRequest request)
    {
        await usuarioService.VerificaUsuarioOrganizacion(User.GetUsername(), id);
        
        var res  = await organizacionService.Editar(id, request);
        return Ok(res);
    }
    
    
}