namespace Models;

public class Organizacion
{
    public int organizacionId { get; set; }
    public string nombre { get; set; }
    public string nombreContacto { get; set; }
    public string telefonoContacto { get; set; }
    public string emailContacto { get; set; }
    public string direccion { get; set; }
    public int comunaId { get; set; }
    public DateTime? fechaEliminacion { get; set; }
    
    public Comuna comuna { get; set; }

    public List<OrganizacionUsuario> organizacionUsuarios { get; set; } = [];
}