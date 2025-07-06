using Models;
using PetsManagerMS.Dtos;
using Shared;

namespace PetsManagerMS.Services;

public interface IAnimalService
{
    Task<PageResult<AnimalResponse>> List(AnimalQuery filter);
    Task<AnimalResponse?> GetById(int id);
    Task<List<AnimalResponse>> GetByUsuario(int usuarioId);
    Task<AnimalResponse?> Crear(AnimalRequest request);
    Task<AnimalResponse?> Editar(int id, AnimalRequest request);
    Task Eliminar(int id);
    Task<List<Sexo>> ListarSexos();
    Task<List<Especie>> ListarEspecies();
    Task<List<Tamano>> ListarTamanos();
    Task<List<NivelActividad>> ListarNivelActividades();
    Task AgregarImagen(int animalId, IFormFile file);
    Task EliminarImagen(int animalId, int imagenId);
}