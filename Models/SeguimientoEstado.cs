namespace Models;

public class SeguimientoEstado
{
    public int seguimientoEstadoId { get; set; }
    public string nombre { get; set; }
}

public enum SeguimientoEstadoEnum
{
    Activo = 10,
    Cerrado = 20
}