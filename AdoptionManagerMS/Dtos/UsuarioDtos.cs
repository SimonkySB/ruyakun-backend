using Models;

namespace AdoptionManagerMS.Dtos;

public class UsuarioResponse
{
    public int usuarioId { get; set; }
    public string username { get; set; }
    public string? nombres { get; set; }
    public string? apellidos { get; set; }
    public string? direccion { get; set; }
    public string? telefono { get; set; }
    public string? telefono2 { get; set; }
    public Comuna? comuna { get; set; }
    
}