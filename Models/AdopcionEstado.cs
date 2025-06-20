namespace Models;

public class AdopcionEstado
{
    public int adopcionEstadoId { get; set; }
    public string nombre { get; set; }
}

public enum AdopcionEstadoEnum
{
    Solicitado = 10,
    EnEvaluacion = 20,
    Adoptado = 30,
    Rechazada = 90,
    Cancelada = 95
    
}