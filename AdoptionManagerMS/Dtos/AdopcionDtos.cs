using System.ComponentModel.DataAnnotations;
using Models;

namespace AdoptionManagerMS.Dtos;

public class AdopcionSolicitarRequest
{
    [Required] public int usuarioId { get; set; }
    [Required] public int animalId { get; set; }
    [Required] public string descripcionFamilia  { get; set; }
}

public class AdopcionQuery
{
    public int? usuarioId { get; set; }
    public int? adopcionEstadoId { get; set; }
    public int? organizacionId { get; set; }
}


public class AdopcionResponse
{
    public int adopcionId { get; set; }
    public DateTime fechaCreacion { get; set; }
    public DateTime fechaActualizacion { get; set; }
    public string descripcionFamilia  { get; set; }
    public AdopcionEstado adopcionEstado { get; set; }
    public UsuarioResponse usuario { get; set; }
    public AnimalResponse animal { get; set; }
}



