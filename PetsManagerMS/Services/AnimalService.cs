using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using PetsManagerMS.Dtos;
using Shared;

namespace PetsManagerMS.Services;


public class AnimalService(AppDbContext db, Cloudinary cloudinary)
{

    public async Task<PageResult<AnimalResponse>> List(AnimalQuery filter)
    {
        var query = db.Animal.AsNoTracking()
            .Include(a => a.organizacion)
                .ThenInclude(o => o.comuna)
            .Include(a => a.especie)
            .Include(a => a.sexo)
            .Include(a => a.nivelActividad)
            .Include(a => a.tamano)
            .Include(a => a.animalImagenes)
            .Where(a => a.fechaEliminacion == null && a.organizacion.fechaEliminacion == null);
        
        if (!string.IsNullOrEmpty(filter.search))
        {
            query = query.Where(a => 
                a.nombre.Contains(filter.search));
        }
        
        

        if (filter.sexoId.HasValue)
        {
            query = query.Where(a => a.sexoId == filter.sexoId);
        }

        if (filter.nivelActividadId.HasValue)
        {
            query = query.Where(a => a.nivelActividadId == filter.nivelActividadId);
        }
        
        if (filter.minEdad.HasValue)
        {
            var minDate = DateTime.Today.AddYears(-filter.minEdad.Value);
            query = query.Where(a => a.fechaNacimiento <= minDate);
        }

        if (filter.maxEdad.HasValue)
        {
            var maxDate = DateTime.Today.AddYears(-filter.maxEdad.Value - 1).AddDays(1);
            query = query.Where(a => a.fechaNacimiento >= maxDate);
        }

        if (filter.tamanoId.HasValue)
        {
            query = query.Where(a => a.tamanoId == filter.tamanoId);
        }

        if (filter.especieId.HasValue)
        {
            query = query.Where(a => a.especieId == filter.especieId);
        }

        if (filter.organizacionId.HasValue)
        {
            query = query.Where(a => a.organizacionId == filter.organizacionId);
        }

        if (filter.comunaId.HasValue)
        {
            query = query.Where(a => a.organizacion.comunaId == filter.comunaId);
        }

        if (filter.publicado.HasValue)
        {
            query = query.Where(a => a.publicado == filter.publicado);
        }
        
        
        query = filter.sortBy?.ToLower() switch
        {
            "fecharegistro" => filter.sortDescending ? query.OrderByDescending(a => a.fechaRegistro) : query.OrderBy(a => a.fechaRegistro),
            _ => query.OrderByDescending(a => a.fechaRegistro)
        };

        var totalCount = await query.CountAsync();
        
        var animales = await query
            .Skip((filter.page - 1) * filter.pageSize)
            .Take(filter.pageSize).ToListAsync();
        
        
        return new PageResult<AnimalResponse>
        {
            items = animales.Select(a => ToAnimalResponse(a)).ToList(),
            totalCount = totalCount,
            page = filter.page,
            pageSize = filter.pageSize
        };
    }
    
    public async Task<AnimalResponse?> GetById(int id)
    {
        var res = await db.Animal.AsNoTracking()
            .Include(a => a.organizacion)
            .ThenInclude(o => o.comuna)
            .Include(a => a.especie)
            .Include(a => a.sexo)
            .Include(a => a.nivelActividad)
            .Include(a => a.tamano)
            .Include(a => a.animalImagenes)
            .Where(a => a.fechaEliminacion == null && a.animalId == id && a.organizacion.fechaEliminacion == null)
            .FirstOrDefaultAsync();
        if (res == null)
        {
            return null;
        }
        return ToAnimalResponse(res);
    }

    public async Task<List<AnimalResponse>> GetByUsuario(int usuarioId)
    {
        var res = await db.Animal.AsNoTracking()
            .Include(a => a.organizacion)
            .ThenInclude(o => o.comuna)
            .Include(a => a.organizacion)
            .ThenInclude(a => a.organizacionUsuarios)
            .Include(a => a.especie)
            .Include(a => a.sexo)
            .Include(a => a.nivelActividad)
            .Include(a => a.tamano)
            .Include(a => a.animalImagenes)
            .Where(a => 
                a.fechaEliminacion == null 
                && a.organizacion.fechaEliminacion == null
                && a.organizacion.organizacionUsuarios.Any(o => o.organizacionId == usuarioId))
            .ToListAsync();

        return res.Select(r => ToAnimalResponse(r)).ToList();
    }
    

    public async Task<AnimalResponse?> Crear(AnimalRequest request)
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
    
    public async Task<AnimalResponse?> Editar(int id, AnimalRequest request)
    {
        await VerifyRequest(request);
        var animal = await GetByIdOrException(id);

        if (request.publicado)
        {
            var adopciones = await db.Adopcion.AsNoTracking()
                .Where(a => a.animalId == id && a.adopcionEstadoId == (int)AdopcionEstadoEnum.Aprobada)
                .ToListAsync();

            if (adopciones.Count > 0)
            {
                throw new AppException("No es posible publicar el animal, ya se encuentra adoptado.");
            }

        }

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


    public async Task AgregarImagen(int animalId, IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new AppException("Archivo no valido");
        }
        
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !extensionesPermitidas.Contains(extension))
        {
            throw new AppException("Extensión de archivo no permitida. Solo se permiten .jpg, .jpeg, .png");
        }

        var animal = await GetByIdOrException(animalId);
        
        
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(nombreArchivo, file.OpenReadStream()),
        };
        var uploadResult = cloudinary.Upload(uploadParams);
        
        string refCode = uploadResult.PublicId;
        string url = uploadResult.Url.OriginalString;
        
        AnimalImagen imagen = new()
        {
            url = url,
            refCode = refCode
        };
        animal.animalImagenes.Add(imagen);
        await db.SaveChangesAsync();
    }

    public async Task EliminarImagen(int animalId, int imagenId)
    {
        var animal = await GetByIdOrException(animalId);
        var imagen = animal.animalImagenes.FirstOrDefault(a => a.animalImagenId == imagenId);
        if (imagen == null)
        {
            throw new AppException("Imagen no existe");
        }
        
        try
        {
            await cloudinary.DeleteResourcesAsync(imagen.refCode);
        }
        catch (Exception e)
        {
            // ignored
        }
        
        db.AnimalImagen.Remove(imagen);
        await db.SaveChangesAsync();
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
            .Include(a => a.animalImagenes)
            .Where(a => a.fechaEliminacion == null && a.animalId == id && a.organizacion.fechaEliminacion == null)
            .FirstOrDefaultAsync();
        if (res == null)
        {
            throw new AppException("Animal no existe");
        }
        return res;
    }


    private  AnimalResponse ToAnimalResponse(Animal animal)
    {
        return new AnimalResponse()
        {
            animalId = animal.animalId,
            nombre = animal.nombre,
            peso = animal.peso,
            fechaRegistro = animal.fechaRegistro,
            fechaNacimiento = animal.fechaNacimiento,
            publicado = animal.publicado,
            descripcion = animal.descripcion,
            especieId = animal.especieId,
            sexoId = animal.sexoId,
            organizacionId = animal.organizacionId,
            fechaEliminacion = animal.fechaEliminacion,
            tamanoId = animal.tamanoId,
            nivelActividadId = animal.nivelActividadId,
            especie = animal.especie,
            sexo = animal.sexo,
            organizacion = new AnimalOrganizacionResponse()
            {
                organizacionId = animal.organizacion.organizacionId,
                nombre = animal.organizacion.nombre,
                nombreContacto = animal.organizacion.nombreContacto,
                telefonoContacto = animal.organizacion.telefonoContacto,
                emailContacto = animal.organizacion.emailContacto,
                direccion = animal.organizacion.direccion,
                comuna = animal.organizacion.comuna,
            },
            tamano = animal.tamano,
            nivelActividad = animal.nivelActividad,
            animalImagenes = animal.animalImagenes
        };
    } 
    
}