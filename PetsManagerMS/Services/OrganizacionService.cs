using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using PetsManagerMS.Dtos;
using Shared;

namespace PetsManagerMS.Services;

public class OrganizacionService(AppDbContext db)
{
    public async Task<List<Organizacion>> List()
    {
        var res = await db.Organizacion.AsNoTracking()
            .Include(o => o.comuna)
            .Where(a => a.fechaEliminacion == null).ToListAsync();
        return res;
    }
    
    public async Task<Organizacion?> GetById(int id)
    {
        var res = await db.Organizacion.AsNoTracking()
            .Include(o => o.comuna)
            .Where(a => a.fechaEliminacion == null && a.organizacionId == id)
            .FirstOrDefaultAsync();
        return res;
    }
    
    public async Task<Organizacion?> Crear(OrganizacionRequest request)
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

    public async Task<Organizacion?> Editar(int id, OrganizacionRequest request)
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
}