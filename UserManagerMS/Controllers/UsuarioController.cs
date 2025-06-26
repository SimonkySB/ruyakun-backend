using Microsoft.AspNetCore.Mvc;
using UserManagerMS.Dtos;
using UserManagerMS.Services;


namespace UserManagerMS.Controllers;


[ApiController]
[Route("usuarios")]
public class UsuarioController(UsuarioService usuarioService) : ControllerBase
{

   [HttpGet]
   public async Task<ActionResult> List([FromQuery] UsuarioQuery query)
   {
      var res = await usuarioService.List(query);
      return Ok(res);
   }

   [HttpGet("{id}")]
   public async Task<ActionResult> GetById(int id)
   {
      var res = await usuarioService.GetById(id);
      
      if (res == null)
      {
         return NotFound("Usuario no encontrado");
      }

      return Ok(res);
   }

   [HttpPost]
   public async Task<ActionResult> Crear(UsuarioRequest request)
   {
      var res = await usuarioService.Crear(request);
      
      return Ok(res);
   }
   
   
   [HttpPut("{id}")]
   public async Task<ActionResult> Editar(int id, UsuarioRequest request)
   {
      var res = await usuarioService.Editar(id, request);
      return Ok(res);
   }
   
}