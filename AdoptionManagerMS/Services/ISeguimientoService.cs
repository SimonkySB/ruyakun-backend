using AdoptionManagerMS.Dtos;
using Models;

namespace AdoptionManagerMS.Services;

public interface ISeguimientoService
{
    Task<List<SeguimientoResponse>> List(SeguimientoQuery query);
    Task<List<SeguimientoResponse>> ListByAdminColaborador(int usuarioId);
    Task<SeguimientoResponse?> GetById(int id);
    Task<SeguimientoResponse?> GetByIdAndUser(int id, int usuarioId);
    Task<SeguimientoResponse?> GetByIdAndAdminColaborador(int id, int usuarioId);
    Task<SeguimientoResponse?> Crear(SeguimientoRequest request);
    Task<SeguimientoResponse?> Editar(int id, SeguimientoRequest request);
    Task Eliminar(int id);
    Task<SeguimientoResponse?> Cerrar(int id, SeguimientoCerrarRequest request);
    Task<List<SeguimientoEstado>> GetEstados();
    Task<List<SeguimientoTipo>> GetTipos();
}