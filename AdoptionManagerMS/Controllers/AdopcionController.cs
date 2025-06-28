using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionManagerMS.Controllers;


[ApiController]
[Route("adopciones")]
public class AdopcionController(AdopcionService adopcionService) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> List([FromQuery] AdopcionQuery query)
    {
        var res = await adopcionService.List(query);
        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var res = await adopcionService.GetById(id);
        if (res == null)
        {
            return NotFound("Adopcion no encontrada");
        }
        return Ok(res);
    }

    [HttpPost("solicitar")]
    public async Task<ActionResult> Solicitar(AdopcionSolicitarRequest request)
    {
        var res = await adopcionService.Solicitar(request);
        return Ok(res);
    }

    [HttpPost("{id}/aprobar")]
    public async Task<ActionResult> Aprobar(int id)
    {
        var res = await adopcionService.Aprobar(id);
        return Ok(res);
    }
  
    [HttpPost("{id}/rechazar")]
    public async Task<ActionResult> Rechazar(int id)
    {
        var res = await adopcionService.Rechazar(id);
        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await adopcionService.Eliminar(id);
        return NoContent();
    }
  

    [HttpGet("estados")]
    public async Task<ActionResult> ListEstados()
    {
        var res = await adopcionService.ListAdopcionEstados();
        return Ok(res);
    }
}