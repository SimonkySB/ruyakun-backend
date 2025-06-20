namespace UserManagerMS.Models;

public class Adopcion
{
    public int adopcionId { get; set; }
    public int usuarioId { get; set; }
    public int adopcionEstadoId { get; set; }
    public DateTime fechaCreacion { get; set; }
    public DateTime fechaActualizacion { get; set; }
    public string descripcionFamilia  { get; set; }
    
    public Usuario usuario { get; set; }
    public AdopcionEstado adopcionEstado { get; set; }
    public List<Seguimiento> seguimientos { get; set; } = [];
}