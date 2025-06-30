using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;

namespace PetsManagerMS.Services;

public class UsuarioService(AppDbContext db)
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
    
    public async Task VerificaUsuarioOrganizacion(string email, int organizacionId)
    {
        var usuario = await VerificaUsuario(email);
        var organizacion = await VerificarOrganizacion(organizacionId, usuario.usuarioId);
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