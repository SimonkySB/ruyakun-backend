using System.ComponentModel.DataAnnotations;

namespace PetsManagerMS.Dtos;

public class OrganizacionRequest
{
    [Required] public string nombre { get; set; }
    [Required] public string nombreContacto { get; set; }
    [Required] public string telefonoContacto { get; set; }
    [Required] [EmailAddress] public string emailContacto { get; set; }
    [Required] public string direccion { get; set; }
    [Required] public int comunaId { get; set; }
}

public class OrganizacionResponse
{
    public required int organizacionId { get; set; }
    public required string nombre { get; set; }
    public required string nombreContacto { get; set; }
    public required string telefonoContacto { get; set; }
    public required string emailContacto { get; set; }
    public required string direccion { get; set; }
    public required DateTime? fechaEliminacion { get; set; }
    public OrganizacionComunaResponse comuna { get; set; }
    public List<int> usuariosId { get; set; }
    
}

public class OrganizacionComunaResponse
{
    public required int comunaId { get; set; }
    public required string nombre { get; set; }
}


