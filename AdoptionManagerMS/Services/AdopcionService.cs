using AdoptionManagerMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;

namespace AdoptionManagerMS.Services;

public class AdopcionService(AppDbContext db)
{
    public async Task<List<Adopcion>> List(AdopcionQuery filter)
    {
        IQueryable<Adopcion> query = db.Adopcion
            .AsNoTracking()
            .Include(a => a.adopcionEstado)
            .Include(a => a.animal);

        
        if (filter.usuarioId.HasValue)
        {
            query = query.Where(q => q.usuarioId == filter.usuarioId);
        }
        if (filter.adopcionEstadoId.HasValue)
        {
            query = query.Where(q => q.adopcionEstadoId == filter.adopcionEstadoId);
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
        
        bool publicado = true;
        var animal = await db.Animal.AsNoTracking().Where(a => 
            a.animalId == request.animalId && a.fechaEliminacion == null && a.publicado == publicado
            ).FirstOrDefaultAsync();

        if (animal == null)
        {
            throw new Exception("Animal no encontrado");
        }

        if (user == null)
        {
            throw new Exception("Usuario no encontrado");
        }

        Adopcion adopcion = new()
        {
            animalId = request.animalId,
            usuarioId = request.usuarioId,
            adopcionEstadoId = (int)AdopcionEstadoEnum.Solicitado,
            fechaCreacion = DateTime.UtcNow,
            fechaActualizacion = DateTime.UtcNow,
            descripcionFamilia = request.descripcionFamilia,
        };
        
        await db.Adopcion.AddAsync(adopcion);
        await db.SaveChangesAsync();
        return await GetById(adopcion.adopcionId);

    }


    public async Task<List<AdopcionEstado>> ListAdopcionEstados()
    {
        return await db.AdopcionEstado.AsNoTracking().ToListAsync();
    }
}