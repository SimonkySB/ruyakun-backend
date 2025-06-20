using System.ComponentModel.DataAnnotations;

namespace UserManagerMS.Dtos;

public class UsuarioRequest
{
    [Required] [EmailAddress] public string username { get; set; }
    [Required] public string nombres { get; set; }
    [Required] public string apellidos { get; set; }
    [Required] public string direccion { get; set; }
    [Required] public string telefono { get; set; }
    [Required] public bool activo { get; set; }
    public string? telefono2 { get; set; }
    [Required] public int comunaId { get; set; }
}