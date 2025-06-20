using System.ComponentModel.DataAnnotations;

namespace PetsManagerMS.Dtos;

public class AnimalRequest
{
    [Required] public string nombre { get; set; }
    [Required] [Range(0, double.MaxValue, ErrorMessage = "El peso debe ser igual o mayor a 0.")] public decimal peso { get; set; }
    [Required] public DateTime fechaNacimiento { get; set; }
    [Required] public bool publicado { get; set; }
    [Required] public string descripcion { get; set; }
    [Required] public int especieId { get; set; }
    [Required] public int sexoId { get; set; }
    [Required] public int organizacionId { get; set; }
    [Required] public int tamanoId { get; set; }
    [Required] public int nivelActividadId { get; set; }
}