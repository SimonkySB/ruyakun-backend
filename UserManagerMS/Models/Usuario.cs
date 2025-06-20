namespace UserManagerMS.Models;

public class Usuario
{
    public int usuarioId { get; set; }
    public string username { get; set; }
    public string nombres { get; set; }
    public string apellidos { get; set; }
    public bool activo { get; set; }
    public DateTime fechaCreacion { get; set; }
    public string direccion { get; set; }
    public string telefono { get; set; }
    public string? telefono2 { get; set; }
    public int comunaId { get; set; }
    
    public Comuna comuna { get; set; }

    public List<OrganizacionUsuario> organizacionUsuarios { get; set; } = [];
}