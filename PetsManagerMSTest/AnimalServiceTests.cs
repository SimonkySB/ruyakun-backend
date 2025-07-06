using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Moq;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;
using Shared;

namespace PetsManagerMSTest;

public class AnimalServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICloudinary> _mockCloudinary;
    private readonly AnimalService _service;
    
    public AnimalServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new AppDbContext(options);
        _mockCloudinary = new Mock<ICloudinary>();
        _service = new AnimalService(_context, _mockCloudinary.Object);
            
        SeedDatabase();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    private void SeedDatabase()
    {
        // Seed communes
        var comuna = new Comuna { comunaId = 1, nombre = "Test Comuna" };
        _context.Comuna.Add(comuna);

        // Seed organizaciones
        var organizacion = new Organizacion
        {
            organizacionId = 1,
            nombre = "Test Org",
            nombreContacto = "Contact",
            telefonoContacto = "123456789",
            emailContacto = "test@org.com",
            direccion = "Test Address",
            comunaId = 1,
            fechaEliminacion = null
        };
        _context.Organizacion.Add(organizacion);

        // Seed catalogs
        var especie = new Especie { especieId = 1, nombre = "Perro" };
        var sexo = new Sexo { sexoId = 1, nombre = "Macho" };
        var tamano = new Tamano { tamanoId = 1, nombre = "Grande" };
        var nivelActividad = new NivelActividad { nivelActividadId = 1, nombre = "Alto" };

        _context.Especie.Add(especie);
        _context.Sexo.Add(sexo);
        _context.Tamano.Add(tamano);
        _context.NivelActividad.Add(nivelActividad);

        // Seed animals
        var animal = new Animal
        {
            animalId = 1,
            nombre = "Test Animal",
            peso = (decimal)10.5,
            fechaRegistro = DateTime.UtcNow,
            fechaNacimiento = DateTime.Today.AddYears(-2),
            publicado = true,
            descripcion = "Test Description",
            especieId = 1,
            sexoId = 1,
            organizacionId = 1,
            tamanoId = 1,
            nivelActividadId = 1,
            fechaEliminacion = null,
            animalImagenes = new List<AnimalImagen>()
        };
        _context.Animal.Add(animal);

        _context.SaveChanges();
    }
    
    
    #region List Tests
    [Fact]
    public async Task List_SinFiltros_RetornaTodosLosAnimales()
    {
        // Arrange
        var filter = new AnimalQuery
        {
            page = 1,
            pageSize = 10
        };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.Should().NotBeNull();
        result.items.Should().HaveCount(1);
        result.totalCount.Should().Be(1);
        result.page.Should().Be(1);
        result.pageSize.Should().Be(10);
        result.items.First().nombre.Should().Be("Test Animal");
    }
    
    [Fact]
    public async Task List_ConFiltroSearch_RetornaAnimalesFiltrados()
    {
        // Arrange
        var filter = new AnimalQuery
        {
            search = "Test",
            page = 1,
            pageSize = 10
        };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.items.Should().HaveCount(1);
        result.items.First().nombre.Should().Contain("Test");
    }
    
    [Fact]
    public async Task List_ConFiltroEdadMinima_RetornaAnimalesFiltrados()
    {
        // Arrange
        var filter = new AnimalQuery
        {
            minEdad = 1,
            page = 1,
            pageSize = 10
        };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.items.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task List_ConFiltroEdadMinima_RetornaAnimalesFiltradosSinResultado()
    {
        // Arrange
        var filter = new AnimalQuery
        {
            minEdad = 90,
            page = 1,
            pageSize = 10
        };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.items.Should().HaveCount(0);
    }
    #endregion
    
    #region GetById Tests

    [Fact]
    public async Task GetById_AnimalExiste_RetornaAnimal()
    {
        // Act
        var result = await _service.GetById(1);

        // Assert
        result.Should().NotBeNull();
        result.animalId.Should().Be(1);
        result.nombre.Should().Be("Test Animal");
        result.organizacion.Should().NotBeNull();
        result.organizacion.nombre.Should().Be("Test Org");
    }

    [Fact]
    public async Task GetById_AnimalNoExiste_RetornaNull()
    {
        // Act
        var result = await _service.GetById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_AnimalEliminado_RetornaNull()
    {
        // Arrange
        var animal = await _context.Animal.FindAsync(1);
        animal.fechaEliminacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetById(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion
    
    #region GetByUsuario Tests

    [Fact]
    public async Task GetByUsuario_UsuarioTieneAnimales_RetornaAnimales()
    {
        // Arrange
        var orgUsuario = new OrganizacionUsuario
        {
            organizacionId = 1,
            usuarioId = 1
        };
        _context.OrganizacionUsuario.Add(orgUsuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByUsuario(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().nombre.Should().Be("Test Animal");
    }

    [Fact]
    public async Task GetByUsuario_UsuarioNoTieneAnimales_RetornaListaVacia()
    {
        // Act
        var result = await _service.GetByUsuario(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
    
    #region Crear Tests

        [Fact]
        public async Task Crear_RequestValido_CreaAnimal()
        {
            // Arrange
            var request = new AnimalRequest
            {
                nombre = "Nuevo Animal",
                peso = (decimal)8.5,
                fechaNacimiento = DateTime.Today.AddYears(-1),
                publicado = false,
                descripcion = "Nueva descripción",
                especieId = 1,
                sexoId = 1,
                organizacionId = 1,
                tamanoId = 1,
                nivelActividadId = 1
            };

            // Act
            var result = await _service.Crear(request);

            // Assert
            result.Should().NotBeNull();
            result.nombre.Should().Be("Nuevo Animal");
            result.peso.Should().Be((decimal)8.5);
            result.publicado.Should().BeFalse();

            var animalEnBd = await _context.Animal.FindAsync(result.animalId);
            animalEnBd.Should().NotBeNull();
            animalEnBd.nombre.Should().Be("Nuevo Animal");
        }

        [Fact]
        public async Task Crear_EspecieNoExiste_LanzaExcepcion()
        {
            // Arrange
            var request = new AnimalRequest
            {
                nombre = "Animal Test",
                peso = (decimal)5.0,
                fechaNacimiento = DateTime.Today.AddYears(-1),
                publicado = false,
                descripcion = "Test",
                especieId = 999, // No existe
                sexoId = 1,
                organizacionId = 1,
                tamanoId = 1,
                nivelActividadId = 1
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.Crear(request));
            exception.Message.Should().Be("Especie no existe");
        }

        [Fact]
        public async Task Crear_OrganizacionNoExiste_LanzaExcepcion()
        {
            // Arrange
            var request = new AnimalRequest
            {
                nombre = "Animal Test",
                peso = (decimal)5.0,
                fechaNacimiento = DateTime.Today.AddYears(-1),
                publicado = false,
                descripcion = "Test",
                especieId = 1,
                sexoId = 1,
                organizacionId = 999, // No existe
                tamanoId = 1,
                nivelActividadId = 1
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.Crear(request));
            exception.Message.Should().Be("Organizacion no existe");
        }

        #endregion
        
        #region Editar Tests

        [Fact]
        public async Task Editar_RequestValido_ActualizaAnimal()
        {
            // Arrange
            var request = new AnimalRequest
            {
                nombre = "Animal Actualizado",
                peso = (decimal)12.0,
                fechaNacimiento = DateTime.Today.AddYears(-3),
                publicado = false,
                descripcion = "Descripción actualizada",
                especieId = 1,
                sexoId = 1,
                organizacionId = 1,
                tamanoId = 1,
                nivelActividadId = 1
            };

            // Act
            var result = await _service.Editar(1, request);

            // Assert
            result.Should().NotBeNull();
            result.nombre.Should().Be("Animal Actualizado");
            result.peso.Should().Be((decimal)12.0);
            result.publicado.Should().BeFalse();

            var animalEnBD = await _context.Animal.FindAsync(1);
            animalEnBD.nombre.Should().Be("Animal Actualizado");
        }

        [Fact]
        public async Task Editar_AnimalNoExiste_LanzaExcepcion()
        {
            // Arrange
            var request = new AnimalRequest
            {
                nombre = "Animal Test",
                peso = (decimal)5.0,
                fechaNacimiento = DateTime.Today.AddYears(-1),
                publicado = false,
                descripcion = "Test",
                especieId = 1,
                sexoId = 1,
                organizacionId = 1,
                tamanoId = 1,
                nivelActividadId = 1
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.Editar(999, request));
            exception.Message.Should().Be("Animal no existe");
        }

        [Fact]
        public async Task Editar_AnimalAdoptadoYPublicado_LanzaExcepcion()
        {
            // Arrange
            var adopcion = new Adopcion
            {
                animalId = 1,
                adopcionEstadoId = (int)AdopcionEstadoEnum.Aprobada,
                fechaActualizacion = DateTime.UtcNow,
                usuarioId = 1,
                descripcionFamilia = "test"
            };
            _context.Adopcion.Add(adopcion);
            await _context.SaveChangesAsync();

            var request = new AnimalRequest
            {
                nombre = "Animal Test",
                peso = (decimal)5.0,
                fechaNacimiento = DateTime.Today.AddYears(-1),
                publicado = true, // Intentando publicar
                descripcion = "Test",
                especieId = 1,
                sexoId = 1,
                organizacionId = 1,
                tamanoId = 1,
                nivelActividadId = 1
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.Editar(1, request));
            exception.Message.Should().Be("No es posible publicar el animal, ya se encuentra adoptado.");
        }

        #endregion
        
        #region Eliminar Tests

        [Fact]
        public async Task Eliminar_AnimalExiste_EliminaAnimal()
        {
            // Act
            await _service.Eliminar(1);

            // Assert
            var animal = await _context.Animal.FindAsync(1);
            animal.fechaEliminacion.Should().NotBeNull();
            animal.fechaEliminacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Eliminar_AnimalNoExiste_LanzaExcepcion()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.Eliminar(999));
            exception.Message.Should().Be("Animal no existe");
        }

        #endregion
        
        #region Catalog Tests

        [Fact]
        public async Task ListarSexos_RetornaListaDeSexos()
        {
            // Act
            var result = await _service.ListarSexos();

            // Assert
            result.Should().HaveCount(1);
            result.First().nombre.Should().Be("Macho");
        }

        [Fact]
        public async Task ListarEspecies_RetornaListaDeEspecies()
        {
            // Act
            var result = await _service.ListarEspecies();

            // Assert
            result.Should().HaveCount(1);
            result.First().nombre.Should().Be("Perro");
        }

        [Fact]
        public async Task ListarTamanos_RetornaListaDeTamanos()
        {
            // Act
            var result = await _service.ListarTamanos();

            // Assert
            result.Should().HaveCount(1);
            result.First().nombre.Should().Be("Grande");
        }

        [Fact]
        public async Task ListarNivelActividades_RetornaListaDeNivelesActividad()
        {
            // Act
            var result = await _service.ListarNivelActividades();

            // Assert
            result.Should().HaveCount(1);
            result.First().nombre.Should().Be("Alto");
        }

        #endregion
        
        #region Image Tests

        [Fact]
        public async Task AgregarImagen_ArchivoValido_AgregaImagen()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var content = "fake image content";
            var fileName = "test.jpg";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);

            var uploadResult = new ImageUploadResult
            {
                PublicId = "test-ref-code",
                Url = new Uri("https://example.com/test.jpg")
            };

            _mockCloudinary.Setup(c => c.Upload(It.IsAny<ImageUploadParams>()))
                .Returns(uploadResult);

            // Act
            await _service.AgregarImagen(1, mockFile.Object);

            // Assert
            var animal = await _context.Animal
                .Include(a => a.animalImagenes)
                .FirstAsync(a => a.animalId == 1);

            animal.animalImagenes.Should().HaveCount(1);
            animal.animalImagenes.First().url.Should().Be("https://example.com/test.jpg");
            animal.animalImagenes.First().refCode.Should().Be("test-ref-code");
        }

        [Fact]
        public async Task AgregarImagen_ArchivoVacio_LanzaExcepcion()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.AgregarImagen(1, mockFile.Object));
            exception.Message.Should().Be("Archivo no valido");
        }

        [Fact]
        public async Task AgregarImagen_ExtensionNoPermitida_LanzaExcepcion()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1000);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.AgregarImagen(1, mockFile.Object));
            exception.Message.Should().Be("Extensión de archivo no permitida. Solo se permiten .jpg, .jpeg, .png");
        }

        [Fact]
        public async Task EliminarImagen_ImagenExiste_EliminaImagen()
        {
            
            // Arrange
            var imagen = new AnimalImagen
            {
                animalImagenId = 1,
                url = "https://example.com/test.jpg",
                refCode = "test-ref-code",
                animalId = 1,
            };

            await _context.AnimalImagen.AddAsync(imagen);
            await _context.SaveChangesAsync();

            _mockCloudinary.Setup(c => c.DeleteResourcesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new DelResResult()));

            // Act
            await _service.EliminarImagen(1, 1);

            // Assert
            var animalActualizado = await _context.Animal
                .Include(a => a.animalImagenes)
                .FirstAsync(a => a.animalId == 1);

            animalActualizado.animalImagenes.Should().BeEmpty();
        }

        [Fact]
        public async Task EliminarImagen_ImagenNoExiste_LanzaExcepcion()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.EliminarImagen(1, 999));
            exception.Message.Should().Be("Imagen no existe");
        }

        #endregion
}