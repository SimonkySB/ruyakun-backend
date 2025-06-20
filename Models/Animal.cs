namespace Models;

public class Animal
{
    public int animalId { get; set; }
    public string nombre { get; set; }
    public decimal peso { get; set; }
    public DateTime fechaRegistro { get; set; }
    public DateTime fechaNacimiento { get; set; }
    public bool publicado { get; set; }
    public string descripcion { get; set; }
    public int especieId { get; set; }
    public int sexoId { get; set; }
    public int organizacionId { get; set; }
    public DateTime? fechaEliminacion { get; set; }
    public int tamanoId { get; set; }
    public int nivelActividadId { get; set; }
    
    public Especie especie { get; set; }
    public Sexo sexo { get; set; }
    public Organizacion organizacion { get; set; }
    public Tamano tamano { get; set; }
    public NivelActividad nivelActividad { get; set; }

    public List<AnimalImagen> animalImagenes { get; set; } = [];
    
    public int edad
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > today.AddYears(-age)) age--;

            return age;
        }
    }

}