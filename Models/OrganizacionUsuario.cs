namespace Models;

public class OrganizacionUsuario
{
    public int organizacionId { get; set; }
    public int usuarioId { get; set; }
    
    public Organizacion organizacion { get; set; }
    public Usuario usuario { get; set; }
}