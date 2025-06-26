using Microsoft.AspNetCore.Mvc;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;

namespace PetsManagerMS.Controllers;

[ApiController]
[Route("organizaciones")]
public class OrganizacionController(OrganizacionService organizacionService) : ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var res = await organizacionService.List();
        return Ok(res);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var res = await organizacionService.GetById(id);
        if (res == null)
        {
            return NotFound("Organizacion no encontrado");
        }
        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(OrganizacionRequest request)
    {
        var res = await organizacionService.Crear(request);
        return Ok(res);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, OrganizacionRequest request)
    {
        var res = await organizacionService.Editar(id, request);
        return Ok(res);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await organizacionService.Eliminar(id);
        return NoContent();
    }


    [HttpPost("{organizacionId}/usuarios/{usuarioId}")]
    public async Task<IActionResult> AgregarUsuario(int organizacionId, int usuarioId)
    {
        await organizacionService.AgregarUsuario(organizacionId, usuarioId);
        return Ok();
    }
    
    [HttpDelete("{organizacionId}/usuarios/{usuarioId}")]
    public async Task<IActionResult> QuitarUsuario(int organizacionId, int usuarioId)
    {
        await organizacionService.QuitarUsuario(organizacionId, usuarioId);
        return NoContent();
    }
}