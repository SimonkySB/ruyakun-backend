using AdoptionManagerMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;

namespace AdoptionManagerMS.Services;



public class SeguimientoService(AppDbContext db)
{
    
    public async Task<List<SeguimientoResponse>> List(SeguimientoQuery query)
    {
        IQueryable<Seguimiento> seguimientos = GetQuery();

        if (query.usuarioId.HasValue)
        {
            seguimientos = seguimientos.Where(u => query.usuarioId == u.adopcion.usuarioId);
        }
            
        if (query.adopcionId.HasValue)
        {
            seguimientos = seguimientos.Where(u => query.adopcionId == u.adopcion.adopcionId);
        }

        var data = await seguimientos.ToListAsync();
        return data.Select(s => ToResponse(s)).ToList();
    }

    public async Task<List<SeguimientoResponse>> ListByAdminColaborador(int usuarioId)
    {
        var data = await GetQuery()
            .Where(s => s.adopcion.animal.organizacion.organizacionUsuarios.Any(o => o.usuarioId == usuarioId))
            .ToListAsync();
        return data.Select(s => ToResponse(s)).ToList();
        
    }

    public async Task<SeguimientoResponse?> GetById(int id)
    {
        var res = await GetQuery()
            .Where(s => s.seguimientoId == id)
            .FirstOrDefaultAsync();
        if (res == null)
        {
            return null;
        }
        return ToResponse(res);
    }
    
    public async Task<SeguimientoResponse?> GetByIdAndUser(int id, int usuarioId)
    {
        var res = await GetQuery()
            .Where(s => s.seguimientoId == id && s.adopcion.usuarioId == usuarioId)
            .FirstOrDefaultAsync();
        if (res == null)
        {
            return null;
        }
        return ToResponse(res);
    }
    
    
    public async Task<SeguimientoResponse?> GetByIdAndAdminColaborador(int id, int usuarioId)
    {
        var res = await GetQuery()
            .Where(s => s.seguimientoId == id && s.adopcion.animal.organizacion.organizacionUsuarios.Any(o => o.usuarioId == usuarioId))
            .FirstOrDefaultAsync();
        if (res == null)
        {
            return null;
        }
        return ToResponse(res);
    }

    public async Task<SeguimientoResponse?> Crear(SeguimientoRequest request)
    {
        
        var seguimientoTipo = await db.SeguimientoTipo.AsNoTracking()
            .FirstOrDefaultAsync(e => e.seguimientoTipoId == request.seguimientoTipoId);

        if (seguimientoTipo == null)
        {
            throw new AppException("Tipo de seguimiento no encontrado");
        }
        
        var adopcion = await db.Adopcion.AsNoTracking()
            .FirstOrDefaultAsync(e => e.adopcionId == request.adopcionId);
        
        if (adopcion == null)
        {
            throw new AppException("Adopcion no encontrada");
        }
        
        Seguimiento seguimiento = new ()
        {
            adopcionId = request.adopcionId,
            seguimientoTipoId = request.seguimientoTipoId,
            seguimientoEstadoId = (int)SeguimientoEstadoEnum.Activo,
            fechaEntrevista = request.fechaInteraccion,
            fechaCreacion = DateTime.UtcNow,
            fechaActualizacion = DateTime.UtcNow,
            descripcion = request.descripcion
        };
        await db.Seguimiento.AddAsync(seguimiento);
        await db.SaveChangesAsync();
        return await GetById(seguimiento.seguimientoId);
    }

    public async Task<SeguimientoResponse?> Editar(int id, SeguimientoRequest request)
    {
        var seguimiento = await db.Seguimiento.FirstOrDefaultAsync(s => s.seguimientoId == id);
        if (seguimiento == null)
        {
            throw new AppException("Seguimiento no encontrado");
        }
        
        seguimiento.descripcion = request.descripcion;
        seguimiento.fechaEntrevista = request.fechaInteraccion;
        seguimiento.fechaActualizacion = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        return await GetById(seguimiento.seguimientoId);
    }

    public async Task Eliminar(int id)
    {
        var seguimiento = await db.Seguimiento.FirstOrDefaultAsync(s => s.seguimientoId == id);
        if (seguimiento == null)
        {
            throw new AppException("Seguimiento no encontrado");
        }
        
        db.Seguimiento.Remove(seguimiento);
        await db.SaveChangesAsync();
        
    } 
    
    
    public async Task<SeguimientoResponse?> Cerrar(int id, SeguimientoCerrarRequest request)
    {
        var seguimiento = await db.Seguimiento.FirstOrDefaultAsync(s => s.seguimientoId == id);
        if (seguimiento == null)
        {
            throw new AppException("Seguimiento no encontrado");
        }

        if (seguimiento.seguimientoEstadoId == (int)SeguimientoEstadoEnum.Cerrado)
        {
            throw new AppException("Seguimiento ya fue cerrado");
        }
        
        seguimiento.fechaCierre = DateTime.UtcNow;
        seguimiento.fechaActualizacion = DateTime.UtcNow;
        seguimiento.seguimientoEstadoId = (int)SeguimientoEstadoEnum.Cerrado;
        seguimiento.observacion = request.observacion;
        
        await db.SaveChangesAsync();
        
        return await GetById(seguimiento.seguimientoId);
    }


    public async Task<List<SeguimientoEstado>> GetEstados()
    {
        return await db.SeguimientoEstado.AsNoTracking().ToListAsync();
    }
    
    public async Task<List<SeguimientoTipo>> GetTipos()
    {
        return await db.SeguimientoTipo.AsNoTracking().ToListAsync();
    }

    
    private IQueryable<Seguimiento> GetQuery()
    {
        return db.Seguimiento.AsNoTracking()
            .Include(s => s.seguimientoEstado)
            .Include(s => s.seguimientoTipo)
            .Include(s => s.adopcion)
            .ThenInclude(s => s.usuario)
            .Include(s => s.adopcion)
            .ThenInclude(a => a.animal)
            .ThenInclude(a => a.organizacion)
            .ThenInclude(a => a.organizacionUsuarios)
            .Include(s => s.adopcion);
    }

    private SeguimientoResponse? ToResponse(Seguimiento s)
    {
        return new SeguimientoResponse
        {
            seguimientoId = s.seguimientoId,
            fechaInteraccion = s.fechaEntrevista,
            fechaCreacion = s.fechaCreacion,
            descripcion = s.descripcion,
            observacion = s.observacion,
            fechaActualizacion = s.fechaActualizacion,
            fechaCierre = s.fechaCierre,
            adopcionId = s.adopcionId,
            animalId = s.adopcion.animalId,
            usuarioId = s.adopcion.usuarioId,
            usuarioNombre = s.adopcion.usuario.nombres,
            animalNombre = s.adopcion.animal.nombre,
            seguimientoTipo = s.seguimientoTipo,
            seguimientoEstado = s.seguimientoEstado,
        };
    }
}