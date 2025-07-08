using Models;

namespace AdoptionManagerMS.Services;

public interface IUsuarioService
{
    Task<Usuario?> GetByEmail(string email);
    Task<Organizacion?> OrganizacionUsuario(int organizacionId, int userId);
    Task VerificaAdopcionUsuario(int adopcionId, int userId);
    Task<Organizacion> VerificarOrganizacion(int organizacionId, int userId);
    Task<Usuario> VerificaUsuario(string email);
}