namespace UserManagerMS.Models;

public class Seguimiento
{
    public int seguimientoId { get; set; }
    public int adopcionId { get; set; }
    public int seguimientoTipoId { get; set; }
    public DateTime fechaCreacion { get; set; }
    public string? descripcion { get; set; }
    public DateTime? fechaEntrevista { get; set; }
    public int seguimientoEstadoId { get; set; }
    public string? observacion {get; set;}
    public DateTime fechaActualizacion { get; set; }
    public DateTime? fechaCierre { get; set; }
    public string? mensajeAdoptante {get; set;}
    
    public Adopcion adopcion { get; set; }
    public SeguimientoTipo seguimientoTipo { get; set; }
    public SeguimientoEstado seguimientoEstado { get; set; }
}