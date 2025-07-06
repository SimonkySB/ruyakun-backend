using PetsManagerMS.Dtos;

namespace PetsManagerMS.Services;

public interface IOrganizacionService
{
    Task<List<OrganizacionResponse>> List(int? usuarioId);
    Task<OrganizacionResponse?> GetById(int id);
    Task<OrganizacionResponse?> Crear(OrganizacionRequest request);
    Task<OrganizacionResponse?> Editar(int id, OrganizacionRequest request);
    Task Eliminar(int id);
    Task AgregarUsuario(int organizacionId, int usuarioId);
    Task QuitarUsuario(int organizacionId, int usuarioId);
}