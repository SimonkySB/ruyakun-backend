using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionManagerMS.Controllers;

[ApiController]
[Route("seguimientos")]
public class SeguimientoController(SeguimientoService seguimientoService) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> GetSeguimientos([FromQuery] SeguimientoQuery query)
    {
        var res = await seguimientoService.List(query);
        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetSeguimiento(int id)
    {
        var res = await seguimientoService.GetById(id);
        if (res == null)
        {
            return NotFound("Seguimiento no encontrado");
        }
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult> CrearSeguimiento(SeguimientoRequest request)
    {
        var res = await seguimientoService.Crear(request);
        return Ok(res);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> EditarSeguimiento(int id, SeguimientoRequest request)
    {
        var res = await seguimientoService.Editar(id, request);
        return Ok(res);
    }

   

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarSeguimiento(int id)
    {
        await seguimientoService.Eliminar(id);
        return NoContent();
    }
    
    [HttpPost("{id}/cerrar")]
    public async Task<ActionResult> CerrarSeguimiento(int id, SeguimientoCerrarRequest request)
    {
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
}