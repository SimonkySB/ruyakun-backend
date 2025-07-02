using AdoptionManagerMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;

namespace AdoptionManagerMS.Services;

public class AdopcionService(AppDbContext db, EventGridService eventGridService)
{
    public async Task<object> List(AdopcionQuery filter)
    {
        IQueryable<Adopcion> query = GetQuery();
        
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
        
        var data = await query.ToListAsync();
        return data.Select(r => ToResponse(r)).ToList();
    }

    public async Task<List<object>> ListByAdminColaborador(int usuarioId)
    {
        var res  = await GetQuery()
            .Where(q => q.animal.organizacion.organizacionUsuarios.Any(a => a.usuarioId == usuarioId))
            .OrderByDescending(q => q.fechaActualizacion)
            .ToListAsync();
        return res.Select(r => ToResponse(r)).ToList();
    }

    public async Task<object?> GetById(int id)
    {
        var adopcion = await GetQuery()
            .Where(q => q.adopcionId == id)
            .FirstOrDefaultAsync();
        if (adopcion == null)
        {
            return null;
        }
        return ToResponse(adopcion);
    }
    
    public async Task<object?> GetByIdAndUser(int id, int userId)
    {
        var adopcion = await GetQuery()
            .Where(q => q.adopcionId == id && q.usuarioId == userId)
            .FirstOrDefaultAsync();
        if (adopcion == null)
        {
            return null;
        }
        return ToResponse(adopcion);
    }
    public async Task<object?> GetByIdAndAdminColaborador(int id, int userId)
    {
        var adopcion = await GetQuery()
            .Where(q => q.adopcionId == id && q.animal.organizacion.organizacionUsuarios.Any(a => a.usuarioId == userId))
            .FirstOrDefaultAsync();
        if (adopcion == null)
        {
            return null;
        }
        return ToResponse(adopcion);
    }
    
    
    public async Task<object?> Solicitar(AdopcionSolicitarRequest request)
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

    public async Task<object?> Aprobar(int id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
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
            
           

            var seguimientoTipo = await db.SeguimientoTipo.AsNoTracking()
                .FirstOrDefaultAsync(s => s.seguimientoTipoId == (int)SeguimientoTipoEnum.VisitaDomiciliaria);
            
            var hoy = DateTime.UtcNow.Date;
            
            var fechaTentativa = hoy.AddDays(7).AddHours(17);

            if (fechaTentativa.DayOfWeek == DayOfWeek.Saturday)
            {
                fechaTentativa = fechaTentativa.AddDays(2);
            }
            else if (fechaTentativa.DayOfWeek == DayOfWeek.Sunday)
            {
                fechaTentativa = fechaTentativa.AddDays(1);
            }

            var seguimiento = new Seguimiento()
            {
                adopcionId = adopcion.adopcionId,
                seguimientoTipoId = (int)SeguimientoTipoEnum.VisitaDomiciliaria,
                seguimientoEstadoId = (int)SeguimientoEstadoEnum.Activo,
                fechaEntrevista = fechaTentativa,
                fechaCreacion = DateTime.UtcNow,
                fechaActualizacion = DateTime.UtcNow,
                descripcion = "Seguimiento Inicial",
            };
            await db.Seguimiento.AddAsync(seguimiento);
            
            
            await eventGridService.PublishEventAsync(
                "Adopcion.Solicitada",
                new
                {
                    emailAdoptante = adopcion.usuario.username,
                    contenido = $"""
                                     <h2>Felicidades {adopcion.usuario.nombres}. Tu solicitud de adopción para {adopcion.animal.nombre} a sido Aprobada!</h2>
                                     <h4>En breve recibiras información de tu próximo seguimiento.</h4>
                                     <p>Si tienes dudas, puedes contactarnos a adopciones@rukayun.cl</p>
                                 """,
                    asunto = "Solicitud de adopcion aprobada!",
                    
                },
                $"adopciones/${adopcion.adopcionId}"
            );


            string? fechaEntrevista = seguimiento.fechaEntrevista?.ToString("dd-MM-yyyy HH:mm");
            await eventGridService.PublishEventAsync(
                "Adopcion.Solicitada",
                new
                {
                    emailAdoptante = adopcion.usuario.username,
                    contenido = $"""
                                     <h2>Se ha programado un evento de tipo: {seguimientoTipo?.nombre} para {adopcion.animal.nombre}</h2>
                                     <p>Detalles del evento</p>
                                     <ul>
                                        <li>Fecha de Interacción: {fechaEntrevista}</li>
                                        <li>Descripción: {seguimiento.descripcion}</li>
                                     </ul>
                                     <p>Si tienes dudas, puedes contactarnos a adopciones@rukayun.cl</p>
                                 """,
                    asunto = "Seguimiento de Tu Animal de Compañia!",
                    
                },
                $"adopciones/${adopcion.adopcionId}"
            );
        
            await transaction.CommitAsync();
            return await GetById(adopcion.adopcionId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<object?> Rechazar(int id)
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
    
    
    public async Task Eliminar(int id)
    {
        var adopcion = await db.Adopcion
            .FirstOrDefaultAsync(a => a.adopcionId == id);
        if (adopcion == null)
        {
            throw new AppException("Adopcion no encontrada");
        }
        db.Adopcion.Remove(adopcion);
        await db.SaveChangesAsync();
    }

    public async Task<List<AdopcionEstado>> ListAdopcionEstados()
    {
        return await db.AdopcionEstado.AsNoTracking().ToListAsync();
    }


    private IQueryable<Adopcion> GetQuery()
    {
        return db.Adopcion
                .AsNoTracking()
                .Include(a => a.adopcionEstado)
                .Include(a => a.usuario)
                .ThenInclude(u => u.comuna)
                .Include(a => a.animal)
                .ThenInclude(a => a.organizacion)
                .ThenInclude(o => o.comuna)
                .Include(a => a.animal)
                .ThenInclude(a => a.animalImagenes)
                .Include(a => a.animal)
                .ThenInclude(a => a.organizacion)
                .ThenInclude(o => o.organizacionUsuarios)
            ;
    }

    
    private object ToResponse(Adopcion adopcion)
    {
        return new
        {
            adopcionId = adopcion.adopcionId,
            fechaCreacion = adopcion.fechaCreacion,
            fechaActualizacion = adopcion.fechaActualizacion,
            descripcionFamilia = adopcion.descripcionFamilia,
            adopcionEstado = adopcion.adopcionEstado,
            usuario = new
            {
                usuarioId = adopcion.usuario.usuarioId,
                username = adopcion.usuario.username,
                nombres = adopcion.usuario.nombres,
                apellidos = adopcion.usuario.apellidos,
                direccion = adopcion.usuario.direccion,
                telefono = adopcion.usuario.telefono,
                telefono2 = adopcion.usuario.telefono2,
                comuna = adopcion.usuario.comuna,
            },
            animal = new
            {
                animalId = adopcion.animal.animalId,
                nombre = adopcion.animal.nombre,
                peso = adopcion.animal.peso,
                fechaNacimiento = adopcion.animal.fechaNacimiento,
                descripcion = adopcion.animal.descripcion,
                especie = adopcion.animal.especie,
                sexo = adopcion.animal.sexo,
                tamano = adopcion.animal.tamano,
                nivelActividad = adopcion.animal.nivelActividad,
                edad = adopcion.animal.edad,
                animalImagenes = adopcion.animal.animalImagenes,
                organizacion = new
                {
                    organizacionId = adopcion.animal.organizacion.organizacionId,
                    nombre = adopcion.animal.organizacion.nombre,
                    nombreContacto = adopcion.animal.organizacion.nombreContacto,
                    telefonoContacto = adopcion.animal.organizacion.telefonoContacto,
                    emailContacto = adopcion.animal.organizacion.emailContacto,
                    direccion = adopcion.animal.organizacion.direccion,
                    comuna = adopcion.animal.organizacion.comuna,
                }
            }
        };
    }
}