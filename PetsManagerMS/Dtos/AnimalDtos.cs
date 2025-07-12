using System.ComponentModel.DataAnnotations;
using Models;

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

public class AnimalResponse
{
    public int animalId { get; set; }
    public string nombre { get; set; }
    public decimal peso { get; set; }
    public DateTime fechaRegistro { get; set; }
    public DateTime fechaNacimiento { get; set; }
    public bool publicado { get; set; }
    public string descripcion { get; set; }
    public int especieId { get; set; }
    public int sexoId { get; set; }
    public int organizacionId { get; set; }
    public DateTime? fechaEliminacion { get; set; }
    public int tamanoId { get; set; }
    public int nivelActividadId { get; set; }
    public int edad { get; set; }
    
    public Especie especie { get; set; }
    public Sexo sexo { get; set; }
    public AnimalOrganizacionResponse organizacion { get; set; }
    public Tamano tamano { get; set; }
    public NivelActividad nivelActividad { get; set; }
    public List<AnimalImagen> animalImagenes { get; set; } = [];
}

public class AnimalOrganizacionResponse
{
    public int organizacionId { get; set; }
    public string nombre { get; set; }
    public string nombreContacto { get; set; }
    public string telefonoContacto { get; set; }
    public string emailContacto { get; set; }
    public string direccion { get; set; }
    public Comuna comuna { get; set; }
}