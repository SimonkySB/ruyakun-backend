namespace Models;

public class AdopcionEstado
{
    public int adopcionEstadoId { get; set; }
    public string nombre { get; set; }
}

public enum AdopcionEstadoEnum
{
    Pendiente = 10,
    Aprobada = 20,
    Rechazada = 90
}