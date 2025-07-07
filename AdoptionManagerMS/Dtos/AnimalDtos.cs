using Models;

namespace AdoptionManagerMS.Dtos;

public class AnimalResponse
{
    public int animalId { get; set; }
    public string nombre { get; set; }
    public decimal peso { get; set; }
    public DateTime fechaNacimiento { get; set; }
    public string descripcion { get; set; }
    public Especie especie { get; set; }
    public Sexo sexo { get; set; }
    public Tamano tamano { get; set; }
    public NivelActividad nivelActividad { get; set; }
    public int edad { get; set; }
    public List<AnimalImagen> animalImagenes { get; set; }
    public OrganizacionResponse organizacion { get; set; }
}

