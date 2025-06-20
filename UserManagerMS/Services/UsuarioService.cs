using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;
using UserManagerMS.Dtos;

namespace UserManagerMS.Services;

public class UsuarioService(AppDbContext db)
{

    public async Task<List<Usuario>> List()
    {
        var res = await db.Usuario
            .AsNoTracking()
            .Include(u => u.comuna)
            .ToListAsync();
      
        return res;
    } 
    
    public async Task<Usuario?> GetById(int id)
    {
        var res = await db.Usuario
            .AsNoTracking()
            .Include(u => u.comuna)
            .FirstOrDefaultAsync(u => u.usuarioId == id);

        return res;
    }

    public async Task<Usuario?> Crear(UsuarioRequest request)
    {
        var exists = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.username == request.username);
        if (exists != null)
        {
            throw new AppException("El nombre de usuario se encuentra en uso");
            
        }
      
        var comuna = await db.Comuna.AsNoTracking().FirstOrDefaultAsync(c => c.comunaId == request.comunaId);
        if (comuna == null)
        {
            throw new AppException("Comuna no encontrada");
        }
        
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
      
        await db.Usuario.AddAsync(user);
        await db.SaveChangesAsync();
        return await GetById(user.usuarioId);
    }

    public async Task<Usuario?> Editar(int id, UsuarioRequest request)
    {
        
        var comuna = await db.Comuna.AsNoTracking().FirstOrDefaultAsync(c => c.comunaId == request.comunaId);
        if (comuna == null)
        {
            throw new AppException("Comuna no encontrada");
        }
        
        var user = await db.Usuario.FirstOrDefaultAsync(u => u.usuarioId == id);

        if (user == null)
        {
            throw new AppException("Usuario no encontrado");
        }
        
        user.nombres = request.nombres;
        user.apellidos = request.apellidos;
        user.direccion = request.direccion;
        user.activo = request.activo;
        user.telefono = request.telefono;
        user.telefono2 = request.telefono2;
        user.comunaId = request.comunaId;
        
        await db.SaveChangesAsync();
        return await GetById(user.usuarioId);
        
    }
    
}