using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Database;
using Shared;
using UserManagerMS.Dtos;
using UserManagerMS.Services;

namespace UserManagerMSTest;

public class UsuarioServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UsuarioService _service;
    
    public UsuarioServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new UsuarioService(_context);

        // Configurar datos de prueba
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        // Crear comunas de prueba
        var comunas = new List<Comuna>
        {
            new Comuna { comunaId = 1, nombre = "Comuna 1" },
            new Comuna { comunaId = 2, nombre = "Comuna 2" }
        };

        // Crear organizaciones de prueba
        var organizaciones = new List<Organizacion>
        {
            new Organizacion 
            {
                organizacionId = 1, 
                nombre = "Org 1",
                nombreContacto = "Org 1",
                telefonoContacto = "Org 1",
                emailContacto = "org1@gmail.com",
                direccion = "Org 1",
                comunaId = 1
            },
            new Organizacion 
            {
                organizacionId = 2, 
                nombre = "Org 2",
                nombreContacto = "Org 2",
                telefonoContacto = "Org 2",
                emailContacto = "org2@gmail.com",
                direccion = "Org 2",
                comunaId = 2
            }
        };

        // Crear usuarios de prueba
        var usuarios = new List<Usuario>
        {
            new Usuario 
            { 
                usuarioId = 1, 
                username = "usuario1@test.com", 
                nombres = "Usuario 1",
                comunaId = 1,
                activo = true,
                fechaCreacion = DateTime.UtcNow.AddDays(-30)
            },
            new Usuario 
            { 
                usuarioId = 2, 
                username = "usuario2@test.com", 
                nombres = "Usuario 2",
                comunaId = 2,
                activo = true,
                fechaCreacion = DateTime.UtcNow.AddDays(-15)
            }
        };

        
        var organizacionUsuarios = new List<OrganizacionUsuario>
        {
            new OrganizacionUsuario { usuarioId = 1, organizacionId = 1 },
            new OrganizacionUsuario { usuarioId = 2, organizacionId = 2 }
        };

        _context.Comuna.AddRange(comunas);
        _context.Organizacion.AddRange(organizaciones);
        _context.Usuario.AddRange(usuarios);
        _context.OrganizacionUsuario.AddRange(organizacionUsuarios);
        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    #region Tests para List
    
    [Fact]
    public async  Task List_SinFiltros_DeberiaRetornarTodosLosUsuarios()
    {
        var query = new UsuarioQuery();
        var result = await _service.List(query);
        
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyHaveUniqueItems(u => u.usuarioId);
    }
    
    [Fact]
    public async Task List_ConFiltroOrganizacion_DeberiaRetornarUsuariosDeLaOrganizacion()
    {
        // Arrange
        var query = new UsuarioQuery { organizacionId = 1 };

        // Act
        var result = await _service.List(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().usuarioId.Should().Be(1);
    }
    
    [Fact]
    public async Task List_ConFiltroOrganizacionInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new UsuarioQuery { organizacionId = 999 };

        // Act
        var result = await _service.List(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    #endregion
    
    #region Tests para GetById
    [Fact]
    public async Task GetById_ConIdExistente_DeberiaRetornarUsuario()
    {
        // Arrange
        int usuarioId = 1;

        // Act
        var result = await _service.GetById(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.usuarioId.Should().Be(usuarioId);
    }
    
    [Fact]
    public async Task GetById_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        int usuarioId = 999;

        // Act
        var result = await _service.GetById(usuarioId);

        // Assert
        result.Should().BeNull();
    }
    #endregion
    
    #region Tests para GetByEmail
    [Fact]
    public async Task GetByEmail_ConEmailExistente_DeberiaRetornarUsuario()
    {
        // Arrange
        string email = "usuario1@test.com";

        // Act
        var result = await _service.GetByEmail(email);

        // Assert
        result.Should().NotBeNull();
        result.username.Should().Be(email);
    }
    
    [Fact]
    public async Task GetByEmail_ConEmailInexistente_DeberiaRetornarNull()
    {
        // Arrange
        string email = "noexiste@test.com";

        // Act
        var result = await _service.GetByEmail(email);

        // Assert
        result.Should().BeNull();
    }

    #endregion
    
    
    #region Tests para Crear
    [Fact]
    public async Task Crear_ConUsuarioValido_DeberiaCrearUsuario()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            username = "nuevo@test.com",
            nombres = "Nuevo Usuario",
            comunaId = 1
        };

        // Act
        var result = await _service.Crear(nuevoUsuario);

        // Assert
        result.Should().NotBeNull();
        result.username.Should().Be("nuevo@test.com");
        result.fechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.comuna.Should().NotBeNull();

        // Verificar que se guardó en la base de datos
        var usuarioEnBd = await _context.Usuario.FindAsync(result.usuarioId);
        usuarioEnBd.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Crear_ConUsernameExistente_DeberiaLanzarAppException()
    {
        // Arrange
        var usuarioConUsernameExistente = new Usuario
        {
            username = "usuario1@test.com", // Username ya existe
            nombres = "Usuario Duplicado",
            comunaId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _service.Crear(usuarioConUsernameExistente));
            
        exception.Message.Should().Be("El nombre de usuario se encuentra en uso");
    }
    
    [Fact]
    public async Task Crear_ConComunaInexistente_DeberiaLanzarAppException()
    {
        // Arrange
        var usuarioConComunaInvalida = new Usuario
        {
            username = "nuevo@test.com",
            nombres = "Usuario con Comuna Inválida",
            comunaId = 999 // Comuna que no existe
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _service.Crear(usuarioConComunaInvalida));
            
        exception.Message.Should().Be("Comuna no encontrada");
    }
    
    [Fact]
    public async Task Crear_ConComunaIdNulo_DeberiaCrearUsuario()
    {
        // Arrange
        var usuarioSinComuna = new Usuario
        {
            username = "sincomuna@test.com",
            nombres = "Usuario Sin Comuna",
            comunaId = null
        };

        // Act
        var result = await _service.Crear(usuarioSinComuna);

        // Assert
        result.Should().NotBeNull();
        result.comunaId.Should().BeNull();
    }
    #endregion
    
    #region Tests para Editar
    [Fact]
    public async Task Editar_ConUsuarioValido_DeberiaEditarUsuario()
    {
        // Arrange
        var usuarioParaEditar = new Usuario
        {
            usuarioId = 1,
            nombres = "Usuario Editado",
            comunaId = 2
        };

        // Act
        var result = await _service.Editar(usuarioParaEditar);

        // Assert
        result.Should().NotBeNull();
        result.usuarioId.Should().Be(1);
        result.nombres.Should().Be("Usuario Editado");
    }
    
    [Fact]
    public async Task Editar_ConUsernameExistente_DeberiaLanzarAppException()
    {
        // Arrange
        var usuarioParaEditar = new Usuario
        {
            usuarioId = 1,
            username = "usuario2@test.com", // Username del usuario 2
            nombres = "Usuario Editado",
            comunaId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _service.Editar(usuarioParaEditar));
            
        exception.Message.Should().Be("El nombre de usuario se encuentra en uso");
    }
    
    [Fact]
    public async Task Editar_ConComunaInexistente_DeberiaLanzarAppException()
    {
        // Arrange
        var usuarioParaEditar = new Usuario
        {
            usuarioId = 1,
            username = "usuarioeditado@test.com",
            nombres = "Usuario Editado",
            comunaId = 999 // Comuna que no existe
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _service.Editar(usuarioParaEditar));
            
        exception.Message.Should().Be("Comuna no encontrada");
    }
    
    [Fact]
    public async Task Editar_ConUsuarioInexistente_DeberiaLanzarAppException()
    {
        // Arrange
        var usuarioInexistente = new Usuario
        {
            usuarioId = 999,
            username = "usuarioeditado@test.com",
            nombres = "Usuario Editado",
            comunaId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _service.Editar(usuarioInexistente));
            
        exception.Message.Should().Be("Usuario no encontrado");
    }
    #endregion
}
