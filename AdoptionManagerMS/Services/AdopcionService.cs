using AdoptionManagerMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;

namespace AdoptionManagerMS.Services;

public class AdopcionService(AppDbContext db, EventGridService eventGridService)
{
    public async Task<List<Adopcion>> List(AdopcionQuery filter)
    {
        IQueryable<Adopcion> query = db.Adopcion
            .AsNoTracking()
            .Include(a => a.adopcionEstado)
            .Include(a => a.animal)
            ;
            

        
        if (filter.usuarioId.HasValue)
        {
            query = query.Where(q => q.usuarioId == filter.usuarioId);
        }
        if (filter.adopcionEstadoId.HasValue)
        {
            query = query.Where(q => q.adopcionEstadoId == filter.adopcionEstadoId);
        }

        if (filter.organizacionId.HasValue)
        {
            query = query.Where(q => q.animal.organizacionId == filter.organizacionId);
        }
        
        query = query.OrderByDescending(q => q.fechaActualizacion);
        
        return await query.ToListAsync();
    }

    public async Task<Adopcion?> GetById(int id)
    {
        var adopcion = await db.Adopcion
            .AsNoTracking()
            .Include(a => a.adopcionEstado)
            .Include(a => a.animal)
            .FirstOrDefaultAsync(a => a.adopcionId == id);
        return adopcion;
    }

    public async Task<Adopcion?> Solicitar(AdopcionSolicitarRequest request)
    {
        var user = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(a => a.usuarioId == request.usuarioId);
        
        var animal = await db.Animal.AsNoTracking().Where(a => 
            a.animalId == request.animalId && a.fechaEliminacion == null
            ).FirstOrDefaultAsync();

        if (animal == null)
        {
            throw new Exception("Animal no encontrado");
        }

        if (animal.publicado == false)
        {
            throw new Exception("El Animal ya no se encuentra disponible");
        }

        if (user == null)
        {
            throw new Exception("Usuario no encontrado");
        }

        Adopcion adopcion = new()
        {
            animalId = request.animalId,
            usuarioId = request.usuarioId,
            adopcionEstadoId = (int)AdopcionEstadoEnum.Pendiente,
            fechaCreacion = DateTime.UtcNow,
            fechaActualizacion = DateTime.UtcNow,
            descripcionFamilia = request.descripcionFamilia,
        };

       
        
        await db.Adopcion.AddAsync(adopcion);
        await db.SaveChangesAsync();
        
        await eventGridService.PublishEventAsync(
            "Adopcion.Solicitada",
            new
            {
                emailAdoptante = user.username,
                contenido = $"""
                    <h2>Felicidades {user.nombres}. Se he registrado tu solicitud para adoptar a {animal.nombre}</h2>
                    <h4>Pronto estaremos en contacto!</h4>
                """,
                asunto = "Solicitud de adopción",
                
            },
            $"adopciones/${adopcion.adopcionId}"
        );
        
        return await GetById(adopcion.adopcionId);

    }

    public async Task<Adopcion?> Aprobar(int id)
    {
        var adopcion = await db.Adopcion
            .Include(a => a.animal)
            .Include(a => a.usuario)
            .FirstOrDefaultAsync(a => a.adopcionId == id);
        
        if (adopcion == null)
        {
            throw new AppException("Adopcion no encontrada");
        }

        if (adopcion.adopcionEstadoId != (int)AdopcionEstadoEnum.Pendiente)
        {
            throw new AppException("No se posible aprobar esta solicitud. La solicitud no se encuentra en estado pendiente.");
        }

        
        if (adopcion.animal.fechaEliminacion != null)
        {
            throw new AppException("El animal se encuentra eliminado");
        }
        
        var adopciones = await db.Adopcion
            .AsNoTracking()
            .Where(a => a.animalId == adopcion.animalId && a.adopcionId != id && adopcion.adopcionEstadoId == (int)AdopcionEstadoEnum.Aprobada)
            .ToListAsync();
        
        if (adopciones.Count > 0)
        {
            throw new AppException("El animal ya se encuentra adoptado");
        }
        
        
        adopcion.adopcionEstadoId = (int)AdopcionEstadoEnum.Aprobada;
        adopcion.fechaActualizacion = DateTime.UtcNow;
        
        adopcion.animal.publicado = false;
        
        await db.SaveChangesAsync();
        
        await eventGridService.PublishEventAsync(
            "Adopcion.Solicitada",
            new
            {
                emailAdoptante = adopcion.usuario.username,
                contenido = $"""
                                 <h2>Felicidades {adopcion.usuario.nombres}. Tu solicitud de adopción para {adopcion.animal.nombre} a sido Aprobada!</h2>
                             """,
                asunto = "Solicitud de adopcion aprobada!",
                
            },
            $"adopciones/${adopcion.adopcionId}"
        );
        
        return await GetById(adopcion.adopcionId);
        
    }

    public async Task<Adopcion?> Rechazar(int id)
    {
        var adopcion = await db.Adopcion
            .Include(a => a.animal)
            .Include(a => a.usuario)
            .FirstOrDefaultAsync(a => a.adopcionId == id);
        
        if (adopcion == null)
        {
            throw new AppException("Adopcion no encontrada");
        }

        if (adopcion.adopcionEstadoId != (int)AdopcionEstadoEnum.Pendiente)
        {
            throw new AppException("No se posible rechazar esta solicitud. La solicitud no se encuentra en estado pendiente.");
        }

        
        if (adopcion.animal.fechaEliminacion != null)
        {
            throw new AppException("El animal se encuentra eliminado");
        }
        
        adopcion.adopcionEstadoId = (int)AdopcionEstadoEnum.Rechazada;
        adopcion.fechaActualizacion = DateTime.UtcNow;
        
        
        await db.SaveChangesAsync();
        
        await eventGridService.PublishEventAsync(
            "Adopcion.Solicitada",
            new
            {
                emailAdoptante = adopcion.usuario.username,
                contenido = $"""
                                 <h2>Lo sentimos {adopcion.usuario.nombres}. Tu solicitud de adopción para {adopcion.animal.nombre} a sido rechazada!</h2>
                             """,
                asunto = "Solicitud de adopcion Rechazada",
                
            },
            $"adopciones/${adopcion.adopcionId}"
        );
        
        return await GetById(adopcion.adopcionId);
        
    }

    public async Task<List<AdopcionEstado>> ListAdopcionEstados()
    {
        return await db.AdopcionEstado.AsNoTracking().ToListAsync();
    }
}