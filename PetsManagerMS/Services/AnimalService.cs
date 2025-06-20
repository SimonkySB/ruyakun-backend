using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using PetsManagerMS.Dtos;
using Shared;

namespace PetsManagerMS.Services;


public class AnimalService(AppDbContext db)
{

    public async Task<List<Animal>> List()
    {
        var res = await db.Animal.AsNoTracking()
            .Include(a => a.organizacion)
            .Include(a => a.especie)
            .Include(a => a.sexo)
            .Include(a => a.nivelActividad)
            .Include(a => a.tamano)
            .Where(a => a.fechaEliminacion == null && a.organizacion.fechaEliminacion == null)
            .ToListAsync();
        return res;
    }
    
    public async Task<Animal?> GetById(int id)
    {
        var res = await db.Animal.AsNoTracking()
            .Include(a => a.especie)
            .Include(a => a.sexo)
            .Include(a => a.nivelActividad)
            .Include(a => a.tamano)
            .Where(a => a.fechaEliminacion == null && a.animalId == id && a.organizacion.fechaEliminacion == null)
            .FirstOrDefaultAsync();
        return res;
    }

    public async Task<Animal?> Crear(AnimalRequest request)
    {

        await VerifyRequest(request);
        
        Animal animal = new()
        {
            nombre = request.nombre,
            peso = request.peso,
            fechaRegistro = DateTime.UtcNow,
            fechaNacimiento = request.fechaNacimiento,
            publicado = request.publicado,
            descripcion = request.descripcion,
            especieId = request.especieId,
            sexoId = request.sexoId,
            organizacionId = request.organizacionId,
            tamanoId = request.tamanoId,
            nivelActividadId = request.nivelActividadId,
        };
        await db.Animal.AddAsync(animal);
        await db.SaveChangesAsync();
        
        return await GetById(animal.animalId);
    }
    
    public async Task<Animal?> Editar(int id, AnimalRequest request)
    {
        await VerifyRequest(request);
        var animal = await GetByIdOrException(id);

        animal.nombre = request.nombre;
        animal.peso = request.peso;
        animal.fechaNacimiento = request.fechaNacimiento;
        animal.publicado = request.publicado;
        animal.descripcion = request.descripcion;
        animal.especieId = request.especieId;
        animal.sexoId = request.sexoId;
        //animal.organizacionId = request.organizacionId;
        animal.tamanoId = request.tamanoId;
        animal.nivelActividadId = request.nivelActividadId;
        
        await db.SaveChangesAsync();
        
        return await GetById(animal.animalId);
    }

    

    public async Task Eliminar(int id)
    {
        var animal = await GetByIdOrException(id);
        animal.fechaEliminacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    
    
    public async Task<List<Sexo>> ListarSexos()
    {
        return await db.Sexo.AsTracking().ToListAsync();
    }
    
    public async Task<List<Especie>> ListarEspecies()
    {
        return await db.Especie.AsTracking().ToListAsync();
    }
    
    public async Task<List<Tamano>> ListarTamanos()
    {
        return await db.Tamano.AsTracking().ToListAsync();
    }
    
    public async Task<List<NivelActividad>> ListarNivelActividades()
    {
        return await db.NivelActividad.AsTracking().ToListAsync();
    }
    
    
    
    //
    
    private async Task VerifyRequest(AnimalRequest request)
    {
        var especie = await db.Especie.AsNoTracking().FirstOrDefaultAsync(e => e.especieId == request.especieId);
        if (especie == null)
        {
            throw new AppException("Especie no existe");
        }
        var sexo = await db.Sexo.AsNoTracking().FirstOrDefaultAsync(e => e.sexoId == request.sexoId);
        if (sexo == null)
        {
            throw new AppException("Sexo no existe");
        }
        var nivelActividad = await db.NivelActividad.AsNoTracking().FirstOrDefaultAsync(e => e.nivelActividadId == request.nivelActividadId);
        if (nivelActividad == null)
        {
            throw new AppException("Nivel de actividad no existe");
        }
        var tamano = await db.Tamano.AsNoTracking().FirstOrDefaultAsync(e => e.tamanoId == request.tamanoId);
        if (tamano == null)
        {
            throw new AppException("Tamaño no existe");
        }
        
        var organizacion = await db.Organizacion.AsNoTracking().FirstOrDefaultAsync(e => e.organizacionId == request.organizacionId && e.fechaEliminacion == null);
        if (organizacion == null)
        {
            throw new AppException("Organizacion no existe");
        }
    }
    
    private async Task<Animal> GetByIdOrException(int id)
    {
        var res = await db.Animal
            .Include(a => a.organizacion)
            .Where(a => a.fechaEliminacion == null && a.animalId == id && a.organizacion.fechaEliminacion == null)
            .FirstOrDefaultAsync();
        if (res == null)
        {
            throw new AppException("Animal no existe");
        }
        return res;
    }
    
    
    
    
}