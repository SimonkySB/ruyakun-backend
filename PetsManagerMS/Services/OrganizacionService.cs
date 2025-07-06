using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using PetsManagerMS.Dtos;
using Shared;

namespace PetsManagerMS.Services;

public class OrganizacionService(AppDbContext db) : IOrganizacionService
{
    public async Task<List<OrganizacionResponse>> List(int? usuarioId)
    {
        var query = db.Organizacion
            .AsNoTracking()
            .Include(o => o.comuna)
            .Include(o => o.organizacionUsuarios)
            .Where(o => o.fechaEliminacion == null);

        if (usuarioId.HasValue)
        {
            query = query.Where(a => a.organizacionUsuarios.Any(org => org.usuarioId == usuarioId.Value));
        }

        var res = await query.ToListAsync();
        return res.Select(r => ToResponse(r)).ToList();
    }
    
    
    public async Task<OrganizacionResponse?> GetById(int id)
    {
        var res = await db.Organizacion.AsNoTracking()
            .Include(o => o.comuna)
            .Include(o => o.organizacionUsuarios)
            .Where(a => a.fechaEliminacion == null && a.organizacionId == id).FirstOrDefaultAsync();

        if (res == null)
        {
            return null;
        }
        return ToResponse(res);
    }
    
    public async Task<OrganizacionResponse?> Crear(OrganizacionRequest request)
    {

        await VerifyRequest(0, request);
        
        Organizacion organizacion = new()
        {
            nombre = request.nombre,
            nombreContacto = request.nombreContacto,
            telefonoContacto = request.telefonoContacto,
            emailContacto = request.emailContacto,
            direccion = request.direccion,
            comunaId = request.comunaId,
        };
        
        await db.Organizacion.AddAsync(organizacion);
        await db.SaveChangesAsync();
        
        return await GetById(organizacion.organizacionId);
    }

    public async Task<OrganizacionResponse?> Editar(int id, OrganizacionRequest request)
    {
        await VerifyRequest(id, request);
        var organizacion = await GetByIdOrException(id);

        organizacion.nombre = request.nombre;
        organizacion.nombreContacto = request.nombreContacto;
        organizacion.telefonoContacto = request.telefonoContacto;
        organizacion.emailContacto = request.emailContacto;
        organizacion.direccion = request.direccion;
        organizacion.comunaId = request.comunaId;
        
        await db.SaveChangesAsync();
        return await GetById(id);
    }
    
    public async Task Eliminar(int id)
    {
        var organizacion = await GetByIdOrException(id);
        
        DateTime fechaEliminacion = DateTime.UtcNow;
        
        organizacion.fechaEliminacion = fechaEliminacion;
        
        var animales = await db.Animal.Where(a => a.organizacionId == id).ToListAsync();

        foreach (var animal in animales)
        {
            animal.fechaEliminacion = fechaEliminacion;
        }
        
        await db.SaveChangesAsync();
    }


    public async Task AgregarUsuario(int organizacionId, int usuarioId)
    {
        var orgUsuario = await db.OrganizacionUsuario
            .Where(o => o.usuarioId == usuarioId)
            .FirstOrDefaultAsync();
        if (orgUsuario != null)
        {
            throw new AppException("El usuario ya se encuentra en una organización");
        }
        
        var org = await db.Organizacion.AsNoTracking()
            .FirstOrDefaultAsync(o => o.organizacionId == organizacionId && o.fechaEliminacion == null);
        if (org == null)
        {
            throw new AppException("No se encuentra la organizacion");
        }
        
        var usuario = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(o => o.usuarioId == usuarioId);
        if (usuario == null)
        {
            throw new AppException("Usuario no existe");
        }
        await db.OrganizacionUsuario.AddAsync(new ()
        {
            organizacionId = organizacionId,
            usuarioId = usuarioId
        });
        
        await db.SaveChangesAsync();
    }
    
    public async Task QuitarUsuario(int organizacionId, int usuarioId)
    {
        
        var orgUsuario = await db.OrganizacionUsuario
            .Where(o => o.organizacionId == organizacionId && o.usuarioId == usuarioId)
            .FirstOrDefaultAsync();
        if (orgUsuario == null)
        {
            throw new AppException("El usuario no se encuentra en la organización");
        }
        db.OrganizacionUsuario.Remove(orgUsuario);
        await db.SaveChangesAsync();
    }

    private async Task VerifyRequest(int id, OrganizacionRequest request)
    {
        var comuna = await db.Comuna.AsNoTracking().FirstOrDefaultAsync(c => c.comunaId == request.comunaId);
        if (comuna == null)
        {
            throw new AppException("Comuna no encontrada");
        }
        
        var exists = await db.Organizacion.AsNoTracking().FirstOrDefaultAsync(a => a.nombre == request.nombre && (id == 0 || a.organizacionId != id));
        if (exists != null)
        {
            throw new AppException("El nombre se encuentra en uso");
        }
    } 
    
    private async Task<Organizacion> GetByIdOrException(int id)
    {
        var res = await db.Organizacion
            .Where(a => a.fechaEliminacion == null && a.organizacionId == id)
            .FirstOrDefaultAsync();
        if (res == null)
        {
            throw new AppException("Organizacion no existe");
        }
        return res;
    }


    private OrganizacionResponse ToResponse(Organizacion o)
    {
        return new OrganizacionResponse
        {
            organizacionId = o.organizacionId,
            nombre = o.nombre,
            nombreContacto = o.nombreContacto,
            telefonoContacto = o.telefonoContacto,
            emailContacto = o.emailContacto,
            direccion = o.direccion,
            fechaEliminacion = o.fechaEliminacion,
            comuna = o.comuna,
            usuariosId = o.organizacionUsuarios.Select(u => u.usuarioId).ToList(),
        };
    }
}