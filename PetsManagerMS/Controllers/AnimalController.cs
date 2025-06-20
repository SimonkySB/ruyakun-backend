using Microsoft.AspNetCore.Mvc;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;

namespace PetsManagerMS.Controllers;

[ApiController]
[Route("animales")]
public class AnimalController(AnimalService animalService) : ControllerBase
{


    [HttpGet]
    public async Task<IActionResult> List()
    {
        var res = await animalService.List();
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
    public async Task<IActionResult> Crear(AnimalRequest request)
    {
        var res = await animalService.Crear(request);
        return Ok(res);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, AnimalRequest request)
    {
        var res = await animalService.Editar(id, request);
        return Ok(res);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
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
    public async Task<IActionResult> AgregarImagen(int animalId, [FromForm] IFormFile file)
    {
        await animalService.AgregarImagen(animalId, file);
        return NoContent();
    }

    [HttpDelete("{animalId}/imagenes/{id}")]
    public async Task<IActionResult> EliminarImagen(int animalId, int id)
    {
        await animalService.EliminarImagen(animalId, id);
        return NoContent();
    }
    
    
}