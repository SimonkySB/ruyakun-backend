using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;
using Shared;

namespace PetsManagerMSTest;

public class OrganizacionServiceTests : IDisposable
{
    
    private readonly AppDbContext _context;
    private readonly OrganizacionService _service;
    
    public OrganizacionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _service = new OrganizacionService(_context);
        
        SeedData();
    }
    
    private void SeedData()
    {
        var comuna1 = new Comuna { comunaId = 1, nombre = "Comuna 1" };
        var comuna2 = new Comuna { comunaId = 2, nombre = "Comuna 2" };
        
        var usuario1 = new Usuario { usuarioId = 1, username = "user1@test.com", nombres = "User", apellidos = "One" };
        var usuario2 = new Usuario { usuarioId = 2, username = "user2@test.com", nombres = "User", apellidos = "Two" };
        
        var organizacion1 = new Organizacion
        {
            organizacionId = 1,
            nombre = "Organizacion 1",
            nombreContacto = "Contact 1",
            telefonoContacto = "123456789",
            emailContacto = "contact1@test.com",
            direccion = "Address 1",
            comunaId = 1,
            fechaEliminacion = null
        };
        
        var organizacion2 = new Organizacion
        {
            organizacionId = 2,
            nombre = "Organizacion 2",
            nombreContacto = "Contact 2",
            telefonoContacto = "987654321",
            emailContacto = "contact2@test.com",
            direccion = "Address 2",
            comunaId = 2,
            fechaEliminacion = null
        };
        
        var organizacionEliminada = new Organizacion
        {
            organizacionId = 3,
            nombre = "Organizacion Eliminada",
            nombreContacto = "Contact Deleted",
            telefonoContacto = "555555555",
            emailContacto = "deleted@test.com",
            direccion = "Deleted Address",
            comunaId = 1,
            fechaEliminacion = DateTime.UtcNow
        };
        
        var orgUsuario1 = new OrganizacionUsuario { organizacionId = 1, usuarioId = 1 };
        
        _context.Comuna.AddRange(comuna1, comuna2);
        _context.Usuario.AddRange(usuario1, usuario2);
        _context.Organizacion.AddRange(organizacion1, organizacion2, organizacionEliminada);
        _context.OrganizacionUsuario.Add(orgUsuario1);
        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    #region Tests para List

    [Fact]
    public async Task List_SinFiltroUsuario_DeberiaRetornarTodasLasOrganizacionesActivas()
    {
        // Act
        var result = await _service.List(null);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.fechaEliminacion == null);
        result.Should().Contain(o => o.nombre == "Organizacion 1");
        result.Should().Contain(o => o.nombre == "Organizacion 2");
        result.Should().NotContain(o => o.nombre == "Organizacion Eliminada");
    }

    [Fact]
    public async Task List_ConFiltroUsuario_DeberiaRetornarSoloOrganizacionesDelUsuario()
    {
        // Act
        var result = await _service.List(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().nombre.Should().Be("Organizacion 1");
        result.First().usuariosId.Should().Contain(1);
    }

    [Fact]
    public async Task List_ConUsuarioSinOrganizaciones_DeberiaRetornarListaVacia()
    {
        // Act
        var result = await _service.List(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
    
    #region Tests para GetById

    [Fact]
    public async Task GetById_ConOrganizacionExistente_DeberiaRetornarOrganizacion()
    {
        // Act
        var result = await _service.GetById(1);

        // Assert
        result.Should().NotBeNull();
        result.organizacionId.Should().Be(1);
        result.nombre.Should().Be("Organizacion 1");
        result.comuna.Should().NotBeNull();
        result.comuna.nombre.Should().Be("Comuna 1");
    }

    [Fact]
    public async Task GetById_ConOrganizacionInexistente_DeberiaRetornarNull()
    {
        // Act
        var result = await _service.GetById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ConOrganizacionEliminada_DeberiaRetornarNull()
    {
        // Act
        var result = await _service.GetById(3);

        // Assert
        result.Should().BeNull();
    }

    #endregion
    
    #region Tests para Crear

    [Fact]
    public async Task Crear_ConRequestValido_DeberiaCrearOrganizacion()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Nueva Organizacion",
            nombreContacto = "New Contact",
            telefonoContacto = "111111111",
            emailContacto = "new@test.com",
            direccion = "New Address",
            comunaId = 1
        };

        // Act
        var result = await _service.Crear(request);

        // Assert
        result.Should().NotBeNull();
        result.nombre.Should().Be(request.nombre);
        result.nombreContacto.Should().Be(request.nombreContacto);
        result.telefonoContacto.Should().Be(request.telefonoContacto);
        result.emailContacto.Should().Be(request.emailContacto);
        result.direccion.Should().Be(request.direccion);
        result.comuna.comunaId.Should().Be(request.comunaId);

        // Verificar que se guardó en la base de datos
        var organizacionEnBb = await _context.Organizacion.FirstOrDefaultAsync(o => o.nombre == request.nombre);
        organizacionEnBb.Should().NotBeNull();
    }

    [Fact]
    public async Task Crear_ConNombreDuplicado_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Organizacion 1", // Nombre ya existente
            nombreContacto = "Contact",
            telefonoContacto = "123456789",
            emailContacto = "test@test.com",
            direccion = "Address",
            comunaId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _service.Crear(request));
    }

    [Fact]
    public async Task Crear_ConComunaInexistente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Nueva Organizacion",
            nombreContacto = "Contact",
            telefonoContacto = "123456789",
            emailContacto = "test@test.com",
            direccion = "Address",
            comunaId = 999 // Comuna inexistente
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Crear(request));
        exception.Message.Should().Be("Comuna no encontrada");
    }

    #endregion
    
    #region Tests para Editar

    [Fact]
    public async Task Editar_ConOrganizacionExistente_DeberiaActualizarOrganizacion()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Organizacion Actualizada",
            nombreContacto = "Updated Contact",
            telefonoContacto = "999999999",
            emailContacto = "updated@test.com",
            direccion = "Updated Address",
            comunaId = 2
        };

        // Act
        var result = await _service.Editar(1, request);

        // Assert
        result.Should().NotBeNull();
        result.organizacionId.Should().Be(1);
        result.nombre.Should().Be(request.nombre);
        result.nombreContacto.Should().Be(request.nombreContacto);
        result.telefonoContacto.Should().Be(request.telefonoContacto);
        result.emailContacto.Should().Be(request.emailContacto);
        result.direccion.Should().Be(request.direccion);
        result.comuna.comunaId.Should().Be(request.comunaId);

        // Verificar que se actualizó en la base de datos
        var organizacionEnBd = await _context.Organizacion.FirstAsync(o => o.organizacionId == 1);
        organizacionEnBd.nombre.Should().Be(request.nombre);
    }

    [Fact]
    public async Task Editar_ConOrganizacionInexistente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Test",
            nombreContacto = "Contact",
            telefonoContacto = "123456789",
            emailContacto = "test@test.com",
            direccion = "Address",
            comunaId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Editar(999, request));
        exception.Message.Should().Be("Organizacion no existe");
    }

    [Fact]
    public async Task Editar_ConNombreDuplicado_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Organizacion 2", // Nombre de otra organización
            nombreContacto = "Contact",
            telefonoContacto = "123456789",
            emailContacto = "test@test.com",
            direccion = "Address",
            comunaId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Editar(1, request));
        exception.Message.Should().Be("El nombre se encuentra en uso");
    }

    #endregion
    
    #region Tests para Eliminar

    [Fact]
    public async Task Eliminar_ConOrganizacionExistente_DeberiaMarcarComoEliminada()
    {
        // Arrange
        var animal = new Animal 
        { 
            animalId = 1, 
            nombre = "Test Animal", 
            organizacionId = 1, 
            fechaEliminacion = null,
            descripcion = "Test descripcion",
        };
        _context.Animal.Add(animal);
        await _context.SaveChangesAsync();

        // Act
        await _service.Eliminar(1);

        // Assert
        var organizacionEliminada = await _context.Organizacion.FirstAsync(o => o.organizacionId == 1);
        organizacionEliminada.fechaEliminacion.Should().NotBeNull();
        organizacionEliminada.fechaEliminacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verificar que los animales también se marcaron como eliminados
        var animalEliminado = await _context.Animal.FirstAsync(a => a.animalId == 1);
        animalEliminado.fechaEliminacion.Should().NotBeNull();
        animalEliminado.fechaEliminacion.Should().Be(organizacionEliminada.fechaEliminacion);
    }

    [Fact]
    public async Task Eliminar_ConOrganizacionInexistente_DeberiaLanzarExcepcion()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.Eliminar(999));
        exception.Message.Should().Be("Organizacion no existe");
    }

    #endregion
    
    #region Tests para AgregarUsuario

    [Fact]
    public async Task AgregarUsuario_ConUsuarioYOrganizacionValidos_DeberiaAgregarUsuario()
    {
        // Act
        await _service.AgregarUsuario(2, 2);

        // Assert
        var orgUsuario = await _context.OrganizacionUsuario
            .FirstOrDefaultAsync(ou => ou.organizacionId == 2 && ou.usuarioId == 2);
        orgUsuario.Should().NotBeNull();
    }

    [Fact]
    public async Task AgregarUsuario_ConUsuarioYaEnOrganizacion_DeberiaLanzarExcepcion()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.AgregarUsuario(2, 1));
        exception.Message.Should().Be("El usuario ya se encuentra en una organización");
    }

    [Fact]
    public async Task AgregarUsuario_ConOrganizacionInexistente_DeberiaLanzarExcepcion()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.AgregarUsuario(999, 2));
        exception.Message.Should().Be("No se encuentra la organizacion");
    }

    [Fact]
    public async Task AgregarUsuario_ConUsuarioInexistente_DeberiaLanzarExcepcion()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.AgregarUsuario(2, 999));
        exception.Message.Should().Be("Usuario no existe");
    }

    #endregion
    
    #region Tests para QuitarUsuario

    [Fact]
    public async Task QuitarUsuario_ConUsuarioEnOrganizacion_DeberiaQuitarUsuario()
    {
        // Act
        await _service.QuitarUsuario(1, 1);

        // Assert
        var orgUsuario = await _context.OrganizacionUsuario
            .FirstOrDefaultAsync(ou => ou.organizacionId == 1 && ou.usuarioId == 1);
        orgUsuario.Should().BeNull();
    }

    [Fact]
    public async Task QuitarUsuario_ConUsuarioNoEnOrganizacion_DeberiaLanzarExcepcion()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.QuitarUsuario(2, 1));
        exception.Message.Should().Be("El usuario no se encuentra en la organización");
    }

    #endregion
}