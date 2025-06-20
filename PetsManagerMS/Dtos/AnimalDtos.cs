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

public class AnimalQuery
{
    public string? search { get; set; }
    public int? minEdad { get; set; }
    public int? maxEdad { get; set; }
    public int? sexoId { get; set; }
    public int? nivelActividadId { get; set; }
    public int? tamanoId { get; set; }
    public int? especieId { get; set; }
    public int? organizacionId { get; set; }
    public int? comunaId { get; set; }
    public bool? publicado { get; set; }
    
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 12;
    public string? sortBy { get; set; } = "fechaRegistro";
    public bool sortDescending { get; set; } = true;
    
}