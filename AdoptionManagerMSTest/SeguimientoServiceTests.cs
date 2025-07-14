using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Moq;
using Shared;

namespace AdoptionManagerMSTest;

public class SeguimientoServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEventGridService> _eventGridServiceMock;
    private readonly SeguimientoService _service;

    public SeguimientoServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _eventGridServiceMock = new Mock<IEventGridService>();
        _service = new SeguimientoService(_context, _eventGridServiceMock.Object);
            
        SeedDatabase();
    }
    
    private void SeedDatabase()
    {
        // Seed SeguimientoEstado
        var estados = new List<SeguimientoEstado>
        {
            new SeguimientoEstado { seguimientoEstadoId = 1, nombre = "Activo" },
            new SeguimientoEstado { seguimientoEstadoId = 2, nombre = "Cerrado" }
        };
        _context.SeguimientoEstado.AddRange(estados);

        // Seed SeguimientoTipo
        var tipos = new List<SeguimientoTipo>
        {
            new SeguimientoTipo { seguimientoTipoId = 1, nombre = "Visita" },
            new SeguimientoTipo { seguimientoTipoId = 2, nombre = "Llamada" }
        };
        _context.SeguimientoTipo.AddRange(tipos);

        // Seed Organizacion
        var organizacion = new Organizacion
        {
            organizacionId = 1,
            nombre = "Refugio Test",
            nombreContacto = "Refugio Test",
            telefonoContacto = "Refugio Test",
            emailContacto = "Refugio Test",
            direccion = "Refugio Test",
            comunaId = 1,
            organizacionUsuarios = new List<OrganizacionUsuario>()
        };
        _context.Organizacion.Add(organizacion);

        // Seed Usuario
        var usuario = new Usuario
        {
            usuarioId = 1,
            nombres = "Juan Perez",
            username = "juan@test.com"
        };
        _context.Usuario.Add(usuario);

        // Seed OrganizacionUsuario
        var orgUsuario = new OrganizacionUsuario
        {
            organizacionId = 1,
            usuarioId = 1,
            organizacion = organizacion,
            usuario = usuario
        };
        _context.OrganizacionUsuario.Add(orgUsuario);
        organizacion.organizacionUsuarios.Add(orgUsuario);

        // Seed Animal
        var animal = new Animal
        {
            animalId = 1,
            nombre = "Firulais",
            organizacionId = 1,
            organizacion = organizacion,
            descripcion = "Firulais Test",
        };
        _context.Animal.Add(animal);

        // Seed Adopcion
        var adopcion = new Adopcion
        {
            adopcionId = 1,
            usuarioId = 1,
            animalId = 1,
            fechaCreacion = DateTime.UtcNow.AddDays(-30),
            fechaActualizacion = DateTime.UtcNow.AddDays(-15),
            usuario = usuario,
            animal = animal,
            descripcionFamilia = "Firulais Test",
        };
        _context.Adopcion.Add(adopcion);

        // Seed Seguimiento
        var seguimiento = new Seguimiento
        {
            seguimientoId = 1,
            adopcionId = 1,
            seguimientoTipoId = 1,
            seguimientoEstadoId = 1,
            fechaEntrevista = DateTime.UtcNow.AddDays(-1),
            fechaCreacion = DateTime.UtcNow.AddDays(-1),
            fechaActualizacion = DateTime.UtcNow.AddDays(-1),
            descripcion = "Seguimiento inicial",
            adopcion = adopcion,
            seguimientoTipo = tipos[0],
            seguimientoEstado = estados[0]
        };
        _context.Seguimiento.Add(seguimiento);

        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    #region List Tests
    [Fact]
    public async Task List_SinFiltros_DebeRetornarTodosSeguimientos()
    {
        // Arrange
        var query = new SeguimientoQuery();

        // Act
        var result = await _service.List(query);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().seguimientoId.Should().Be(1);
        result.First().usuarioNombre.Should().Be("Juan Perez");
        result.First().animalNombre.Should().Be("Firulais");
    }
    
    [Fact]
    public async Task List_ConFiltroUsuarioId_DebeRetornarSeguimientosDelUsuario()
    {
        // Arrange
        var query = new SeguimientoQuery { usuarioId = 1 };

        // Act
        var result = await _service.List(query);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().usuarioId.Should().Be(1);
    }
    
    [Fact]
    public async Task List_ConFiltroAdopcionId_DebeRetornarSeguimientosDeLaAdopcion()
    {
        // Arrange
        var query = new SeguimientoQuery { adopcionId = 1 };

        // Act
        var result = await _service.List(query);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().adopcionId.Should().Be(1);
    }
    [Fact]
    public async Task List_ConFiltroUsuarioIdInexistente_DebeRetornarListaVacia()
    {
        // Arrange
        var query = new SeguimientoQuery { usuarioId = 999 };

        // Act
        var result = await _service.List(query);

        // Assert
        result.Should().BeEmpty();
    }
    #endregion
    
    #region GetById test
    [Fact]
    public async Task GetById_ConIdExistente_DebeRetornarSeguimiento()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.seguimientoId.Should().Be(1);
        result.descripcion.Should().Be("Seguimiento inicial");
    }

    [Fact]
    public async Task GetById_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var id = 999;

        // Act
        var result = await _service.GetById(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAndUser_ConIdYUsuarioCorrectos_DebeRetornarSeguimiento()
    {
        // Arrange
        var id = 1;
        var usuarioId = 1;

        // Act
        var result = await _service.GetByIdAndUser(id, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.seguimientoId.Should().Be(1);
        result.usuarioId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAndUser_ConUsuarioIncorrecto_DebeRetornarNull()
    {
        // Arrange
        var id = 1;
        var usuarioId = 999;

        // Act
        var result = await _service.GetByIdAndUser(id, usuarioId);

        // Assert
        result.Should().BeNull();
    }

    #endregion
    
    #region Crear Tests
    [Fact]
    public async Task Crear_ConDatosValidos_DebeCrearSeguimiento()
    {
        // Arrange
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "Nuevo seguimiento"
        };

        // Act
        var result = await _service.Crear(request);

        // Assert
        var seguimientoEnDb = await _context.Seguimiento
            .FirstOrDefaultAsync(s => s.seguimientoId == 2);
        seguimientoEnDb.Should().NotBeNull();
        seguimientoEnDb.descripcion.Should().Be("Nuevo seguimiento");
        seguimientoEnDb.adopcionId.Should().Be(1);
        seguimientoEnDb.seguimientoTipoId.Should().Be(1);
        seguimientoEnDb.seguimientoEstadoId.Should().Be((int)SeguimientoEstadoEnum.Activo); // Activo
        
        // Verify events were published
        _eventGridServiceMock.Verify(x => x.PublishEventAsync(
                "Adopcion.Solicitada",
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task Crear_ConTipoInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 999,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "Nuevo seguimiento"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Crear(request));
        exception.Message.Should().Be("Tipo de seguimiento no encontrado");
    }

    [Fact]
    public async Task Crear_ConAdopcionInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var request = new SeguimientoRequest
        {
            adopcionId = 999,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "Nuevo seguimiento"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Crear(request));
        exception.Message.Should().Be("Adopcion no encontrada");
    }
    #endregion
    
    #region Editar Tests
    [Fact]
    public async Task Editar_ConIdExistente_DebeActualizarSeguimiento()
    {
        // Arrange
        var id = 1;
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow.AddDays(1),
            descripcion = "Descripción actualizada"
        };

        // Act
        var result = await _service.Editar(id, request);

        // Assert
        result.Should().NotBeNull();
        result.descripcion.Should().Be("Descripción actualizada");
        result.fechaInteraccion.Should().BeCloseTo(request.fechaInteraccion.Value, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Editar_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = 999;
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "Descripción actualizada"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Editar(id, request));
        exception.Message.Should().Be("Seguimiento no encontrado");
    }
    #endregion
    
    #region Eliminar Tests
    [Fact]
    public async Task Eliminar_ConIdExistente_DebeEliminarSeguimiento()
    {
        // Arrange
        var id = 1;

        // Act
        await _service.Eliminar(id);

        // Assert
        var seguimiento = await _context.Seguimiento.FindAsync(id);
        seguimiento.Should().BeNull();
    }

    [Fact]
    public async Task Eliminar_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Eliminar(id));
        exception.Message.Should().Be("Seguimiento no encontrado");
    }
    #endregion
    
    #region Cerrar Tests
    [Fact]
    public async Task Cerrar_ConIdExistente_DebeCerrarSeguimiento()
    {
        // Arrange
        var id = 1;
        var request = new SeguimientoCerrarRequest
        {
            observacion = "Seguimiento cerrado exitosamente"
        };

        // Act
        var result = await _service.Cerrar(id, request);

        // Assert
        var seguimientoEnDb = await _context.Seguimiento
            .FirstOrDefaultAsync(s => s.seguimientoId == id);
        seguimientoEnDb.Should().NotBeNull();
        seguimientoEnDb.seguimientoEstadoId.Should().Be((int)SeguimientoEstadoEnum.Cerrado);
        seguimientoEnDb.observacion.Should().Be("Seguimiento cerrado exitosamente");
        seguimientoEnDb.fechaCierre.Should().NotBeNull();
    }

    [Fact]
    public async Task Cerrar_ConSeguimientoYaCerrado_DebeLanzarExcepcion()
    {
        // Arrange
        var seguimiento = await _context.Seguimiento.FindAsync(1);
        seguimiento.seguimientoEstadoId = (int)SeguimientoEstadoEnum.Cerrado; // Cerrado
        await _context.SaveChangesAsync();

        var request = new SeguimientoCerrarRequest
        {
            observacion = "Intento de cerrar nuevamente"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Cerrar(1, request));
        exception.Message.Should().Be("Seguimiento ya fue cerrado");
    }

    [Fact]
    public async Task Cerrar_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = 999;
        var request = new SeguimientoCerrarRequest
        {
            observacion = "Observación"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Cerrar(id, request));
        exception.Message.Should().Be("Seguimiento no encontrado");
    }
    #endregion
    
    #region Datos Tests
    [Fact]
    public async Task GetEstados_DebeRetornarTodosLosEstados()
    {
        // Act
        var result = await _service.GetEstados();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.nombre == "Activo");
        result.Should().Contain(e => e.nombre == "Cerrado");
    }

    [Fact]
    public async Task GetTipos_DebeRetornarTodosLosTipos()
    {
        // Act
        var result = await _service.GetTipos();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.nombre == "Visita");
        result.Should().Contain(t => t.nombre == "Llamada");
    }

    #endregion
}