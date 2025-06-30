using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Shared;
using UserManagerMS.Dtos;
using UserManagerMS.Services;


namespace UserManagerMS.Controllers;


[ApiController]
[Route("usuarios")]
[Authorize]
public class UsuarioController(UsuarioService usuarioService) : ControllerBase
{

   [HttpGet]
   public async Task<ActionResult> List([FromQuery] UsuarioQuery query)
   {
      var res = await usuarioService.List(query);
      return Ok(res.Select(r => ToResponse(r)));
   }

   [HttpGet("{id}")]
   public async Task<ActionResult> GetById(int id)
   {
      var res = await usuarioService.GetById(id);
      
      if (res == null)
      {
         return NotFound("Usuario no encontrado");
      }

      return Ok(ToResponse(res));
   }

   [HttpPost]
   [Authorize(Policy = Policies.SuperAdmin)]
   public async Task<ActionResult> Crear(UsuarioRequest request)
   {
      Usuario user = new()
      {
         username = request.username,
         nombres = request.nombres,
         apellidos = request.apellidos,
         activo = request.activo,
         fechaCreacion = DateTime.UtcNow,
         direccion = request.direccion,
         telefono = request.telefono,
         telefono2 = request.telefono2,
         comunaId = request.comunaId
      };
      var res = await usuarioService.Crear(user);
      
      return Ok(ToResponse(res));
   }
   
   
   [HttpPut("{id}")]
   [Authorize(Policy = Policies.SuperAdmin)]
   public async Task<ActionResult> Editar(int id, UsuarioRequest request)
   {
      var usuario = await usuarioService.GetById(id);
      
      if (usuario == null)
      {
         return NotFound("Usuario no encontrado");
      }
      
      usuario.nombres = request.nombres;
      usuario.apellidos = request.apellidos;
      usuario.direccion = request.direccion;
      usuario.activo = request.activo;
      usuario.telefono = request.telefono;
      usuario.telefono2 = request.telefono2;
      usuario.comunaId = request.comunaId;
      
      var res = await usuarioService.Editar(usuario);
      
      return Ok(ToResponse(res));
   }
   
   


   [HttpGet("perfil")]
   public async Task<ActionResult> GetPerfil()
   {
      string username = User.GetUsername();
      string rol = User.GetUserRole();
      var usuario = await usuarioService.GetByEmail(username);
      if (usuario == null)
      {
         return NotFound("Usuario no encontrado");
      }
      return Ok(ToResponse(usuario, rol));
   }

   [HttpPut("perfil")]
   public async Task<ActionResult> EditarPerfil(UsuarioProfileRequest request)
   {
      string username = User.GetUsername();
      string rol = User.GetUserRole();
      var usuario = await usuarioService.GetByEmail(username);
      
      if (usuario == null)
      {
         return NotFound("Usuario no encontrado");
      }
      
      usuario.nombres = request.nombres;
      usuario.apellidos = request.apellidos;
      usuario.direccion = request.direccion;
      usuario.telefono = request.telefono;
      usuario.telefono2 = request.telefono2;
      usuario.comunaId = request.comunaId;
      
      var res = await usuarioService.Editar(usuario);  
      return Ok(ToResponse(res, rol));
   }
   

   [HttpPost("perfil/verificar")]
   public async Task<ActionResult> Verificar()
   {
      string username = User.GetUsername();
      string name = User.GetName();
      string surname = User.GetSurname();
      string rol = User.GetUserRole();
      
      var usuario = await usuarioService.GetByEmail(username);
      if (usuario == null)
      {
         var nuevo = await usuarioService.Crear(new()
         {
            username = username,
            nombres = name,
            apellidos = surname,
            activo = true,
            fechaCreacion = DateTime.UtcNow,
            direccion = "",
            telefono = "",
            telefono2 = "",
            comunaId = null
         });
         return Ok(ToResponse(nuevo, rol));
      }
      else
      {
         usuario.nombres = name;
         usuario.apellidos = surname;
         var res = await usuarioService.Editar(usuario);  
         return Ok(ToResponse(res, rol));
      }
      
   }


   private UsuarioResponse? ToResponse(Usuario? u, string rol = "")
   {
      if (u == null)
      {
         return null;
      }
      return new UsuarioResponse()
      {
         usuarioId = u.usuarioId,
         username = u.username,
         nombres = u.nombres,
         apellidos = u.apellidos,
         activo = u.activo,
         fechaCreacion = u.fechaCreacion,
         direccion = u.direccion,
         telefono = u.telefono,
         telefono2 = u.telefono2,
         comuna = u.comuna,
         rol = rol
      };
   }
}