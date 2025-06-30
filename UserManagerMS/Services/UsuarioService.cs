using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;
using UserManagerMS.Dtos;

namespace UserManagerMS.Services;

public class UsuarioService(AppDbContext db)
{

    public async Task<List<Usuario>> List(UsuarioQuery query)
    {
        IQueryable<Usuario> res = db.Usuario
            .AsNoTracking()
            .Include(u => u.comuna)
            .Include(u => u.organizacionUsuarios);


        if (query.organizacionId.HasValue)
        {
            res = res.Where(u => u.organizacionUsuarios
                .Count(uo => uo.organizacionId == query.organizacionId.Value) > 0);
        }

        return await res.ToListAsync();
    }

    public async Task<Usuario?> GetById(int id)
    {
        return await db.Usuario
            .Include(u => u.comuna)
            .FirstOrDefaultAsync(u => u.usuarioId == id);
    }

    public async Task<Usuario?> GetByEmail(string email)
    {
        var res = await db.Usuario
            .Include(u => u.comuna)
            .FirstOrDefaultAsync(u => u.username == email);
        return res;
    }

    public async Task<Usuario?> Crear(Usuario usuario)
    {
        usuario.fechaCreacion = DateTime.UtcNow;
        
        var exists = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.username == usuario.username);
        if (exists != null)
        {
            throw new AppException("El nombre de usuario se encuentra en uso");

        }

        var comuna = await db.Comuna.AsNoTracking().FirstOrDefaultAsync(c => c.comunaId == usuario.comunaId);
        if (usuario.comunaId != null && comuna == null)
        {
            throw new AppException("Comuna no encontrada");
        }
        
        await db.Usuario.AddAsync(usuario);
        await db.SaveChangesAsync();
        return await GetById(usuario.usuarioId);
    }

    public async Task<Usuario?> Editar(Usuario usuario)
    {

        var exists = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.username == usuario.username && u.usuarioId != usuario.usuarioId);
        if (exists != null)
        {
            throw new AppException("El nombre de usuario se encuentra en uso");

        }
        
        var comuna = await db.Comuna.AsNoTracking().FirstOrDefaultAsync(c => c.comunaId == usuario.comunaId);
        if (usuario.comunaId != null && comuna == null)
        {
            throw new AppException("Comuna no encontrada");
        }

        var user = await db.Usuario.AsTracking().FirstOrDefaultAsync(u => u.usuarioId == usuario.usuarioId);

        if (user == null)
        {
            throw new AppException("Usuario no encontrado");
        }
        
        await db.SaveChangesAsync();
        return await GetById(user.usuarioId);

    }


}