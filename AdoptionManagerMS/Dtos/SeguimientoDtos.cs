using System.ComponentModel.DataAnnotations;
using Models;

namespace AdoptionManagerMS.Dtos;

public class SeguimientoQuery
{
    public int? usuarioId { get; set; }
    public int? adopcionId { get; set; }
}

public class SeguimientoRequest
{
    [Required] public int adopcionId { get; set; }
    [Required] public int seguimientoTipoId { get; set; }
    [Required] public DateTime? fechaInteraccion { get; set; }
    public string? descripcion { get; set; }
    
}

public class SeguimientoCerrarRequest
{
    public string observacion { get; set; }
}

public class SeguimientoResponse
{
    public required int seguimientoId { get; set; }
    public required DateTime? fechaInteraccion { get; set; }
    public required DateTime fechaCreacion { get; set; }
    public required string? descripcion { get; set; }
    public required string? observacion { get; set; }
    public required DateTime fechaActualizacion { get; set; }
    public required DateTime? fechaCierre { get; set; }
    public required int adopcionId { get; set; }
    public required int animalId { get; set; }
    public required int usuarioId { get; set; }
    public required string? usuarioNombre { get; set; }
    public required string animalNombre { get; set; }
    public SeguimientoTipo seguimientoTipo { get; set; }
    public SeguimientoEstado seguimientoEstado { get; set; }
    
}