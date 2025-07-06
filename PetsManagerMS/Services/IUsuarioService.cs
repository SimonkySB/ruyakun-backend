using Models;

namespace PetsManagerMS.Services;

public interface IUsuarioService
{
    Task<Usuario?> GetByEmail(string email);
    Task<Organizacion?> OrganizacionUsuario(int organizacionId, int userId);
    Task VerificaUsuarioOrganizacion(string email, int organizacionId);
    Task<Organizacion> VerificarOrganizacion(int organizacionId, int userId);
    Task<Usuario> VerificaUsuario(string email);
}