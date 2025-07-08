using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;

namespace AdoptionManagerMS.Services;

public class UsuarioService(AppDbContext db) : IUsuarioService
{
    public async Task<Usuario?> GetByEmail(string email)
    {
        var res = await db.Usuario
            .Include(u => u.comuna)
            .FirstOrDefaultAsync(u => u.username == email);
        return res;
    }
    
    public async Task<Organizacion?> OrganizacionUsuario(int organizacionId, int userId)
    {
        var res = await db.OrganizacionUsuario
            .Where(ou => organizacionId == ou.organizacionId && ou.usuarioId == userId)
            .Include(o => o.usuario)
            .Select(o => o.organizacion)
            .FirstOrDefaultAsync();
        return res;
    }
    
    public async Task VerificaAdopcionUsuario(int adopcionId, int userId)
    {
        var adop = await db.Adopcion
            .Include(a => a.animal)
            .Where(a => a.adopcionId == adopcionId)
            .FirstOrDefaultAsync();
        if (adop == null)
        {
            throw new AppException("Adopción inválida");
        }
        await VerificarOrganizacion(adop.animal.organizacionId, userId);
        
    }

    public async Task<Organizacion> VerificarOrganizacion(int organizacionId, int userId)
    {
        var pertenece = await OrganizacionUsuario(organizacionId, userId);
        if (pertenece == null)
        {
            throw new AppException("Usuario no pertenece a la organizacion");
        }
        return pertenece;
    }

    public async Task<Usuario> VerificaUsuario(string email)
    {
        var usuario = await GetByEmail(email);
        if (usuario == null)
        {
            throw new AppException("Usuario no se encuentra registrado");
        }
        return usuario;
    }
}