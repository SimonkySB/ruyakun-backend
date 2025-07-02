namespace Models;


public class SeguimientoTipo
{
    public int seguimientoTipoId { get; set; }
    public string nombre { get; set; }
}

public enum SeguimientoTipoEnum
{
    VisitaDomiciliaria = 10,
    CorreoElectronico = 20,
    ReunionVirtual = 30
        
}