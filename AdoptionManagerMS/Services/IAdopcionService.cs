using AdoptionManagerMS.Dtos;
using Models;

namespace AdoptionManagerMS.Services;

public interface IAdopcionService
{
    Task<List<AdopcionResponse>> List(AdopcionQuery filter);
    Task<List<AdopcionResponse>> ListByAdminColaborador(int usuarioId);
    Task<AdopcionResponse?> GetById(int id);
    Task<AdopcionResponse?> GetByIdAndUser(int id, int userId);
    Task<AdopcionResponse?> GetByIdAndAdminColaborador(int id, int userId);
    Task<AdopcionResponse?> Solicitar(AdopcionSolicitarRequest request);
    Task<AdopcionResponse?> Aprobar(int id);
    Task<AdopcionResponse?> Rechazar(int id);
    Task Eliminar(int id);
    Task<List<AdopcionEstado>> ListAdopcionEstados();
}