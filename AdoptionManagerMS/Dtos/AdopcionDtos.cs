using System.ComponentModel.DataAnnotations;

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