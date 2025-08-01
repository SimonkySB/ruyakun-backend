using Models;

namespace AdoptionManagerMS.Dtos;

public class OrganizacionResponse
{
    public int organizacionId { get; set; }
    public string nombre { get; set; }
    public string nombreContacto { get; set; }
    public string telefonoContacto { get; set; }
    public string emailContacto { get; set; }
    public string direccion { get; set; }
    public Comuna comuna { get; set; }
    
}