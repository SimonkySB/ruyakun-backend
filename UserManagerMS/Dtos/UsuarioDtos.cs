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


public class UsuarioQuery
{
    public int? organizacionId { get; set; }
}

public class UsuarioResponse
{
    public required int usuarioId { get; set; }
    public required string username { get; set; }
    public required string nombres { get; set; }
    public required string apellidos { get; set; }
    public required bool activo { get; set; }
    public required DateTime fechaCreacion { get; set; }
    public required string direccion { get; set; }
    public required string telefono { get; set; }
    public required string? telefono2 { get; set; }
    public required UsuarioComunaResponse comuna { get; set; }
    
}

public class UsuarioComunaResponse
{
    public required int comunaId { get; set; }
    public required string nombre { get; set; }
}


