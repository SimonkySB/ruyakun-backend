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