using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Models;
using Models.Database;
using Moq;
using Shared;

namespace AdoptionManagerMSTest;

public class AdopcionServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEventGridService> _eventGridServiceMock;
    private readonly AdopcionService _service;
    
    public AdopcionServiceTests()
    {

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new AppDbContext(options);
        _eventGridServiceMock = new Mock<IEventGridService>();
        _service = new AdopcionService(_context, _eventGridServiceMock.Object);
        
        SeedData();
    }
    
    private void SeedData()
    {
        // Seed Comunas
        var comuna1 = new Comuna { comunaId = 1, nombre = "Comuna 1" };
        var comuna2 = new Comuna { comunaId = 2, nombre = "Comuna 2" };
        _context.Comuna.AddRange(comuna1, comuna2);

        // Seed Estados
        var estadoPendiente = new AdopcionEstado 
        { 
            adopcionEstadoId = (int)AdopcionEstadoEnum.Pendiente, 
            nombre = "Pendiente" 
        };
        var estadoAprobada = new AdopcionEstado 
        { 
            adopcionEstadoId = (int)AdopcionEstadoEnum.Aprobada, 
            nombre = "Aprobada" 
        };
        var estadoRechazada = new AdopcionEstado 
        { 
            adopcionEstadoId = (int)AdopcionEstadoEnum.Rechazada, 
            nombre = "Rechazada" 
        };
        _context.AdopcionEstado.AddRange(estadoPendiente, estadoAprobada, estadoRechazada);

        // Seed Seguimiento Tipos
        var seguimientoTipo = new SeguimientoTipo
        {
            seguimientoTipoId = (int)SeguimientoTipoEnum.VisitaDomiciliaria,
            nombre = "Visita Domiciliaria"
        };
        _context.SeguimientoTipo.Add(seguimientoTipo);

        // Seed Usuarios
        var usuario1 = new Usuario
        {
            usuarioId = 1,
            username = "test@example.com",
            nombres = "Juan",
            apellidos = "Pérez",
            direccion = "Calle 123",
            telefono = "123456789",
            telefono2 = "987654321",
            comunaId = 1,
            comuna = comuna1
        };
        var usuario2 = new Usuario
        {
            usuarioId = 2,
            username = "admin@example.com",
            nombres = "Admin",
            apellidos = "User",
            direccion = "Admin Street",
            telefono = "111111111",
            comunaId = 2,
            comuna = comuna2
        };
        _context.Usuario.AddRange(usuario1, usuario2);

        // Seed Organizaciones
        var organizacion1 = new Organizacion
        {
            organizacionId = 1,
            nombre = "Refugio Test",
            nombreContacto = "Contacto Test",
            telefonoContacto = "123123123",
            emailContacto = "refugio@test.com",
            direccion = "Refugio 123",
            comunaId = 1,
            comuna = comuna1
        };
        _context.Organizacion.Add(organizacion1);

        // Seed OrganizacionUsuarios
        var orgUsuario = new OrganizacionUsuario
        {
            organizacionId = 1,
            usuarioId = 2,
            organizacion = organizacion1,
            usuario = usuario2
        };
        _context.OrganizacionUsuario.Add(orgUsuario);

        
        // Seed catalogs
        var especie = new Especie { especieId = 1, nombre = "Perro" };
        var sexo = new Sexo { sexoId = 1, nombre = "Macho" };
        var tamano = new Tamano { tamanoId = 1, nombre = "Grande" };
        var nivelActividad = new NivelActividad { nivelActividadId = 1, nombre = "Alto" };
        
        _context.Especie.Add(especie);
        _context.Sexo.Add(sexo);
        _context.Tamano.Add(tamano);
        _context.NivelActividad.Add(nivelActividad);
        
        // Seed Animales
        var animal1 = new Animal
        {
            animalId = 1,
            nombre = "Firulais",
            peso = 10.5m,
            fechaNacimiento = DateTime.Now.AddYears(-2),
            descripcion = "Perro muy amigable",
            especieId = 1,
            sexoId = 1,
            tamanoId = 1,
            nivelActividadId = 1,
            publicado = true,
            organizacionId = 1,
            organizacion = organizacion1
        };
        var animal2 = new Animal
        {
            animalId = 2,
            nombre = "Michi",
            peso = 4.5m,
            fechaNacimiento = DateTime.Now.AddYears(-1),
            descripcion = "Gato muy tranquilo",
            especieId = 1,
            sexoId = 1,
            tamanoId = 1,
            nivelActividadId = 1,
            publicado = true,
            organizacionId = 1,
            organizacion = organizacion1
        };
        _context.Animal.AddRange(animal1, animal2);

        // Seed Adopciones
        var adopcion1 = new Adopcion
        {
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1,
            adopcionEstadoId = (int)AdopcionEstadoEnum.Pendiente,
            fechaCreacion = DateTime.UtcNow.AddDays(-5),
            fechaActualizacion = DateTime.UtcNow.AddDays(-5),
            descripcionFamilia = "Familia con experiencia",
            usuario = usuario1,
            animal = animal1,
            adopcionEstado = estadoPendiente
        };
        _context.Adopcion.Add(adopcion1);

        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    #region List Tests
    [Fact]
    public async Task List_ShouldReturnAllAdopciones_WhenNoFilterApplied()
    {
        // Arrange
        var filter = new AdopcionQuery();

        // Act
        var result = await _service.List(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task List_ShouldFilterByUsuarioId_WhenUsuarioIdProvided()
    {
        // Arrange
        var filter = new AdopcionQuery { usuarioId = 1 };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task List_ShouldFilterByAdopcionEstadoId_WhenEstadoIdProvided()
    {
        // Arrange
        var filter = new AdopcionQuery { adopcionEstadoId = (int)AdopcionEstadoEnum.Pendiente };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task List_ShouldFilterByOrganizacionId_WhenOrganizacionIdProvided()
    {
        // Arrange
        var filter = new AdopcionQuery { organizacionId = 1 };

        // Act
        var result = await _service.List(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task ListByAdminColaborador_ShouldReturnAdopciones_WhenUserIsAdminColaborador()
    {
        // Arrange
        var usuarioId = 2;

        // Act
        var result = await _service.ListByAdminColaborador(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }
    
    #endregion
    
    #region GetById Tests
    [Fact]
    public async Task GetById_ShouldReturnAdopcion_WhenAdopcionExists()
    {
        // Arrange
        var adopcionId = 1;

        // Act
        var result = await _service.GetById(adopcionId);

        // Assert
        result.Should().NotBeNull();
        result.adopcionId.Should().Be(adopcionId);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenAdopcionDoesNotExist()
    {
        // Arrange
        var adopcionId = 999;

        // Act
        var result = await _service.GetById(adopcionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAndUser_ShouldReturnAdopcion_WhenUserOwnsAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuarioId = 1;

        // Act
        var result = await _service.GetByIdAndUser(adopcionId, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.adopcionId.Should().Be(adopcionId);
    }

    [Fact]
    public async Task GetByIdAndUser_ShouldReturnNull_WhenUserDoesNotOwnAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuarioId = 999;

        // Act
        var result = await _service.GetByIdAndUser(adopcionId, usuarioId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAndAdminColaborador_ShouldReturnAdopcion_WhenUserIsAdminColaborador()
    {
        // Arrange
        var adopcionId = 1;
        var usuarioId = 2;

        // Act
        var result = await _service.GetByIdAndAdminColaborador(adopcionId, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.adopcionId.Should().Be(adopcionId);
    }
    #endregion
    
    #region Solicitar Tests
    [Fact]
    public async Task Solicitar_ShouldCreateAdopcion_WhenValidRequest()
    {
        // Arrange
        var request = new AdopcionSolicitarRequest
        {
            animalId = 2,
            usuarioId = 1,
            descripcionFamilia = "Familia responsable"
        };

        // Act
        var result = await _service.Solicitar(request);

        // Assert
        result.Should().NotBeNull();
        
        result.animal.animalId.Should().Be(request.animalId);
        result.usuario.usuarioId.Should().Be(request.usuarioId);
        
        _eventGridServiceMock.Verify(x => x.PublishEventAsync(
            "Adopcion.Solicitada",
            It.IsAny<object>(),
            It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Solicitar_ShouldThrowException_WhenAnimalNotFound()
    {
        // Arrange
        var request = new AdopcionSolicitarRequest
        {
            animalId = 999,
            usuarioId = 1,
            descripcionFamilia = "Familia responsable"
        };

        // Act & Assert
        await _service.Invoking(s => s.Solicitar(request))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Animal no encontrado");
    }

    [Fact]
    public async Task Solicitar_ShouldThrowException_WhenAnimalNotPublished()
    {
        // Arrange
        var animal = await _context.Animal.FindAsync(2);
        animal!.publicado = false;
        await _context.SaveChangesAsync();

        var request = new AdopcionSolicitarRequest
        {
            animalId = 2,
            usuarioId = 1,
            descripcionFamilia = "Familia responsable"
        };

        // Act & Assert
        await _service.Invoking(s => s.Solicitar(request))
            .Should().ThrowAsync<Exception>()
            .WithMessage("El Animal ya no se encuentra disponible");
    }

    [Fact]
    public async Task Solicitar_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var request = new AdopcionSolicitarRequest
        {
            animalId = 2,
            usuarioId = 999,
            descripcionFamilia = "Familia responsable"
        };

        // Act & Assert
        await _service.Invoking(s => s.Solicitar(request))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Usuario no encontrado");
    }
    #endregion
    
    #region Aprobar Tests
    [Fact]
    public async Task Aprobar_ShouldApproveAdopcion_WhenValidRequest()
    {
        // Arrange
        var adopcionId = 1;

        // Act
        var result = await _service.Aprobar(adopcionId);

        // Assert
        result.Should().NotBeNull();
        result.adopcionEstado.adopcionEstadoId.Should().Be((int)AdopcionEstadoEnum.Aprobada);

        // Verify animal is no longer published
        var animal = await _context.Animal.FindAsync(1);
        animal!.publicado.Should().BeFalse();

        
        // Verify seguimiento was created
        var seguimiento = await _context.Seguimiento
            .FirstOrDefaultAsync(s => s.adopcionId == adopcionId);
        seguimiento.Should().NotBeNull();

        // Verify events were published
        _eventGridServiceMock.Verify(x => x.PublishEventAsync(
                "Adopcion.Solicitada",
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Aprobar_ShouldThrowException_WhenAdopcionNotFound()
    {
        // Arrange
        var adopcionId = 999;

        // Act & Assert
        await _service.Invoking(s => s.Aprobar(adopcionId))
            .Should().ThrowAsync<AppException>()
            .WithMessage("Adopcion no encontrada");
    }

    [Fact]
    public async Task Aprobar_ShouldThrowException_WhenAdopcionNotPending()
    {
        // Arrange
        var adopcion = await _context.Adopcion.FindAsync(1);
        adopcion!.adopcionEstadoId = (int)AdopcionEstadoEnum.Aprobada;
        await _context.SaveChangesAsync();

        // Act & Assert
        await _service.Invoking(s => s.Aprobar(1))
            .Should().ThrowAsync<AppException>()
            .WithMessage("No se posible aprobar esta solicitud. La solicitud no se encuentra en estado pendiente.");
    }
    
    [Fact]
    public async Task Aprobar_ShouldScheduleFollowUpCorrectly_WhenApproving()
    {
        // Arrange
        var adopcionId = 1;
        var expectedDate = DateTime.UtcNow.Date.AddDays(7).AddHours(17);

        // Adjust for weekends
        if (expectedDate.DayOfWeek == DayOfWeek.Saturday)
        {
            expectedDate = expectedDate.AddDays(2);
        }
        else if (expectedDate.DayOfWeek == DayOfWeek.Sunday)
        {
            expectedDate = expectedDate.AddDays(1);
        }

        // Act
        await _service.Aprobar(adopcionId);
        
        // Assert
        var seguimiento = await _context.Seguimiento
            .FirstOrDefaultAsync(s => s.adopcionId == adopcionId);
        
        seguimiento.Should().NotBeNull();
        seguimiento!.fechaEntrevista.Should().BeCloseTo(expectedDate, TimeSpan.FromMinutes(1));
        seguimiento.seguimientoTipoId.Should().Be((int)SeguimientoTipoEnum.VisitaDomiciliaria);
        seguimiento.seguimientoEstadoId.Should().Be((int)SeguimientoEstadoEnum.Activo);
    }

    
    #endregion
    
    #region Rechazar Tests
    [Fact]
    public async Task Rechazar_ShouldRejectAdopcion_WhenValidRequest()
    {
        // Arrange
        var adopcionId = 1;

        // Act
        var result = await _service.Rechazar(adopcionId);

        // Assert
        result.Should().NotBeNull();
        result.adopcionEstado.adopcionEstadoId.Should().Be((int)AdopcionEstadoEnum.Rechazada);

        _eventGridServiceMock.Verify(x => x.PublishEventAsync(
                "Adopcion.Solicitada",
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Rechazar_ShouldThrowException_WhenAdopcionNotFound()
    {
        // Arrange
        var adopcionId = 999;

        // Act & Assert
        await _service.Invoking(s => s.Rechazar(adopcionId))
            .Should().ThrowAsync<AppException>()
            .WithMessage("Adopcion no encontrada");
    }

    [Fact]
    public async Task Rechazar_ShouldThrowException_WhenAdopcionNotPending()
    {
        // Arrange
        var adopcion = await _context.Adopcion.FindAsync(1);
        adopcion!.adopcionEstadoId = (int)AdopcionEstadoEnum.Aprobada;
        await _context.SaveChangesAsync();

        // Act & Assert
        await _service.Invoking(s => s.Rechazar(1))
            .Should().ThrowAsync<AppException>()
            .WithMessage("No se posible rechazar esta solicitud. La solicitud no se encuentra en estado pendiente.");
    }
    #endregion
    
    #region Eliminar Tests
    [Fact]
    public async Task Eliminar_ShouldDeleteAdopcion_WhenAdopcionExists()
    {
        // Arrange
        var adopcionId = 1;

        // Act
        await _service.Eliminar(adopcionId);

        // Assert
        var adopcion = await _context.Adopcion.FindAsync(adopcionId);
        adopcion.Should().BeNull();
    }

    [Fact]
    public async Task Eliminar_ShouldThrowException_WhenAdopcionNotFound()
    {
        // Arrange
        var adopcionId = 999;

        // Act & Assert
        await _service.Invoking(s => s.Eliminar(adopcionId))
            .Should().ThrowAsync<AppException>()
            .WithMessage("Adopcion no encontrada");
    }
    #endregion
    
    #region List Estados
    [Fact]
    public async Task ListAdopcionEstados_ShouldReturnAllEstados()
    {
        // Act
        var result = await _service.ListAdopcionEstados();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(e => e.adopcionEstadoId == (int)AdopcionEstadoEnum.Pendiente);
        result.Should().Contain(e => e.adopcionEstadoId == (int)AdopcionEstadoEnum.Aprobada);
        result.Should().Contain(e => e.adopcionEstadoId == (int)AdopcionEstadoEnum.Rechazada);
    }
    #endregion
}