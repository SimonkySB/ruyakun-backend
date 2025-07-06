using Models;
using UserManagerMS.Dtos;

namespace UserManagerMS.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> List(UsuarioQuery query);
    Task<Usuario?> GetById(int id);
    Task<Usuario?> GetByEmail(string email);
    Task<Usuario?> Crear(Usuario usuario);
    Task<Usuario?> Editar(Usuario usuario);
}