using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;
using Shared;

namespace PetsManagerMS.Controllers;

[ApiController]
[Route("animales")]
public class AnimalController(IAnimalService animalService, IUsuarioService usuarioService) : ControllerBase
{


    [HttpGet]
    public async Task<IActionResult> List([FromQuery]AnimalQuery query)
    {
        var res = await animalService.List(query);
        return Ok(res);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var res = await animalService.GetById(id);
        if (res == null)
        {
            return NotFound("Animal no encontrado");
        }
        return Ok(res);
    }

    [HttpPost]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> Crear(AnimalRequest request)
    {

        if (User.ISSuperAdmin())
        {
            var resAdmin = await animalService.Crear(request);
            return Ok(resAdmin);
        }
        
        await usuarioService.VerificaUsuarioOrganizacion(User.GetUsername(), request.organizacionId);
        
        var res = await animalService.Crear(request);
        return Ok(res);
    }
    
    [HttpPut("{id}")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> Editar(int id, AnimalRequest request)
    {
        if (User.ISSuperAdmin())
        {
            var resAdmin = await animalService.Editar(id, request);
            return Ok(resAdmin);
        }
        await usuarioService.VerificaUsuarioOrganizacion(User.GetUsername(), request.organizacionId);
        var res = await animalService.Editar(id, request);
        return Ok(res);
    }
    
    [HttpDelete("{id}")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var res = await animalService.GetById(id);
        if (res == null)
        {
            return NotFound("Animal no encontrado");
        }

        if (User.ISSuperAdmin())
        {
            await animalService.Eliminar(id);
            return NoContent();
        }
        
        await usuarioService.VerificaUsuarioOrganizacion(User.GetUsername(), res.organizacionId);
        await animalService.Eliminar(id);
        return NoContent();
    }


    [HttpGet("sexos")]
    public async Task<IActionResult> ListarSexos()
    {
        var res = await animalService.ListarSexos();
        return Ok(res);
    }
    
    [HttpGet("especies")]
    public async Task<IActionResult> ListarEspecies()
    {
        var res = await animalService.ListarEspecies();
        return Ok(res);
    }
    
    [HttpGet("tamanos")]
    public async Task<IActionResult> ListarTamanos()
    {
        var res = await animalService.ListarTamanos();
        return Ok(res);
    }
    
    [HttpGet("nivelActividades")]
    public async Task<IActionResult> ListarNivelActividades()
    {
        var res = await animalService.ListarNivelActividades();
        return Ok(res);
    }

    [HttpPost("{animalId}/imagenes")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> AgregarImagen(int animalId, [FromForm] IFormFile file)
    {
        var res = await animalService.GetById(animalId);
        if (res == null)
        {
            return NotFound("Animal no encontrado");
        }

        if (User.ISSuperAdmin())
        {
            await animalService.AgregarImagen(animalId, file);
            return NoContent();
        }
        await usuarioService.VerificaUsuarioOrganizacion(User.GetUsername(), res.organizacionId);
        
        await animalService.AgregarImagen(animalId, file);
        return NoContent();
    }

    [HttpDelete("{animalId}/imagenes/{id}")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> EliminarImagen(int animalId, int id)
    {
        var res = await animalService.GetById(animalId);
        if (res == null)
        {
            return NotFound("Animal no encontrado");
        }

        if (User.ISSuperAdmin())
        {
            await animalService.EliminarImagen(animalId, id);
            return NoContent();
        }
        await usuarioService.VerificaUsuarioOrganizacion(User.GetUsername(), res.organizacionId);
        await animalService.EliminarImagen(animalId, id);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(policy: Policies.SuperAdminOrAdminOrColaborador)]
    public async Task<IActionResult> ListAnimalesUsuario()
    {
        var usuario = await usuarioService.VerificaUsuario(User.GetUsername());
        var animales = await animalService.GetByUsuario(usuario.usuarioId);
        return Ok(animales);
    }
    
}