using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Moq;
using Shared;
using UserManagerMS.Controllers;
using UserManagerMS.Dtos;
using UserManagerMS.Services;

namespace UserManagerMSTest;

public class UsuarioControllerTests
{
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly UsuarioController _controller;
    
    public UsuarioControllerTests()
    {
        _mockUsuarioService = new Mock<IUsuarioService>();
        _controller = new UsuarioController(_mockUsuarioService.Object);
            
        // Configurar contexto HTTP básico
        SetupHttpContext();
    }
    
    private void SetupHttpContext(string username = "test@test.com", string role = "ADMIN")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("name", "Test"),
            new Claim("surname", "User"),
            new Claim("emails", username),
            new Claim("extension_Roles", role),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
    
    #region Tests para List

    [Fact]
    public async Task List_DeberiaRetornarOkConUsuarios()
    {
        // Arrange
        var query = new UsuarioQuery();
        var usuarios = new List<Usuario>
        {
            new Usuario { usuarioId = 1, username = "user1@test.com", nombres = "User", apellidos = "One" },
            new Usuario { usuarioId = 2, username = "user2@test.com", nombres = "User", apellidos = "Two" }
        };

        _mockUsuarioService.Setup(s => s.List(query))
            .ReturnsAsync(usuarios);

        // Act
        var result = await _controller.List(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as IEnumerable<UsuarioResponse>;
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task List_ConListaVacia_DeberiaRetornarOkConListaVacia()
    {
        // Arrange
        var query = new UsuarioQuery();
        var usuarios = new List<Usuario>();

        _mockUsuarioService.Setup(s => s.List(query))
            .ReturnsAsync(usuarios);

        // Act
        var result = await _controller.List(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as IEnumerable<UsuarioResponse>;
        response.Should().BeEmpty();
    }

    #endregion
    
    #region Tests para GetById
    [Fact]
    public async Task GetById_ConUsuarioExistente_DeberiaRetornarOk()
    {
        // Arrange
        int usuarioId = 1;
        var usuario = new Usuario 
        { 
            usuarioId = usuarioId, 
            username = "test@test.com", 
            nombres = "Test", 
            apellidos = "User" 
        };

        _mockUsuarioService.Setup(s => s.GetById(usuarioId))
            .ReturnsAsync(usuario);

        // Act
        var result = await _controller.GetById(usuarioId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.usuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public async Task GetById_ConUsuarioInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        int usuarioId = 999;
        _mockUsuarioService.Setup(s => s.GetById(usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.GetById(usuarioId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Usuario no encontrado");
    }
    #endregion
    
    #region Tests para Crear

    [Fact]
    public async Task Crear_ConRequestValido_DeberiaRetornarOk()
    {
        // Arrange
        var request = new UsuarioRequest
        {
            username = "nuevo@test.com",
            nombres = "Nuevo",
            apellidos = "Usuario",
            activo = true,
            direccion = "Test Address",
            telefono = "123456789",
            telefono2 = "987654321",
            comunaId = 1
        };

        var usuarioCreado = new Usuario
        {
            usuarioId = 1,
            username = request.username,
            nombres = request.nombres,
            apellidos = request.apellidos,
            activo = request.activo,
            direccion = request.direccion,
            telefono = request.telefono,
            telefono2 = request.telefono2,
            comunaId = request.comunaId,
            fechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.Crear(It.IsAny<Usuario>()))
            .ReturnsAsync(usuarioCreado);

        // Act
        var result = await _controller.Crear(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.username.Should().Be(request.username);

        // Verificar que se llamó al servicio con los datos correctos
        _mockUsuarioService.Verify(s => s.Crear(It.Is<Usuario>(u => 
            u.username == request.username &&
            u.nombres == request.nombres &&
            u.apellidos == request.apellidos &&
            u.activo == request.activo &&
            u.direccion == request.direccion &&
            u.telefono == request.telefono &&
            u.telefono2 == request.telefono2 &&
            u.comunaId == request.comunaId
        )), Times.Once);
    }

    [Fact]
    public async Task Crear_CuandoServicioLanzaExcepcion_DeberiaPropagarExcepcion()
    {
        // Arrange
        var request = new UsuarioRequest
        {
            username = "duplicado@test.com",
            nombres = "Usuario",
            apellidos = "Duplicado"
        };

        _mockUsuarioService.Setup(s => s.Crear(It.IsAny<Usuario>()))
            .ThrowsAsync(new AppException("El nombre de usuario se encuentra en uso"));

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _controller.Crear(request));
    }

    #endregion
        
    #region Tests para Editar

    [Fact]
    public async Task Editar_ConUsuarioExistente_DeberiaRetornarOk()
    {
        // Arrange
        int usuarioId = 1;
        var usuarioExistente = new Usuario 
        { 
            usuarioId = usuarioId, 
            username = "existing@test.com",
            nombres = "Existing",
            apellidos = "User"
        };

        var request = new UsuarioRequest
        {
            nombres = "Updated",
            apellidos = "User",
            activo = true,
            direccion = "Updated Address",
            telefono = "111111111",
            telefono2 = "222222222",
            comunaId = 2
        };

        var usuarioActualizado = new Usuario
        {
            usuarioId = usuarioId,
            username = usuarioExistente.username,
            nombres = request.nombres,
            apellidos = request.apellidos,
            activo = request.activo,
            direccion = request.direccion,
            telefono = request.telefono,
            telefono2 = request.telefono2,
            comunaId = request.comunaId
        };

        _mockUsuarioService.Setup(s => s.GetById(usuarioId))
            .ReturnsAsync(usuarioExistente);
        _mockUsuarioService.Setup(s => s.Editar(It.IsAny<Usuario>()))
            .ReturnsAsync(usuarioActualizado);

        // Act
        var result = await _controller.Editar(usuarioId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.nombres.Should().Be(request.nombres);

        // Verificar que se actualizaron los campos correctos
        _mockUsuarioService.Verify(s => s.Editar(It.Is<Usuario>(u => 
            u.usuarioId == usuarioId &&
            u.nombres == request.nombres &&
            u.apellidos == request.apellidos &&
            u.activo == request.activo &&
            u.direccion == request.direccion &&
            u.telefono == request.telefono &&
            u.telefono2 == request.telefono2 &&
            u.comunaId == request.comunaId
        )), Times.Once);
    }

    [Fact]
    public async Task Editar_ConUsuarioInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        int usuarioId = 999;
        var request = new UsuarioRequest { nombres = "Test" };

        _mockUsuarioService.Setup(s => s.GetById(usuarioId))
            .ReturnsAsync((Usuario)null);

        // Act
        var result = await _controller.Editar(usuarioId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Usuario no encontrado");

        // Verificar que no se intentó editar
        _mockUsuarioService.Verify(s => s.Editar(It.IsAny<Usuario>()), Times.Never);
    }

    #endregion
        
    #region Tests para GetPerfil

    [Fact]
    public async Task GetPerfil_ConUsuarioExistente_DeberiaRetornarOk()
    {
        // Arrange
        var usuario = new Usuario 
        { 
            usuarioId = 1, 
            username = "test@test.com", 
            nombres = "Test", 
            apellidos = "User" 
        };

        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync(usuario);

        // Act
        var result = await _controller.GetPerfil();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.username.Should().Be("test@test.com");
        response.rol.Should().Be("ADMIN");
    }

    [Fact]
    public async Task GetPerfil_ConUsuarioInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.GetPerfil();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
        
    #region Tests para Verificar

    [Fact]
    public async Task Verificar_ConUsuarioNuevo_DeberiaCrearYRetornarOk()
    {
        // Arrange
        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync((Usuario?)null);

        var usuarioCreado = new Usuario 
        { 
            usuarioId = 1, 
            username = "test@test.com", 
            nombres = "Test", 
            apellidos = "User",
            activo = true,
            fechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.Crear(It.IsAny<Usuario>()))
            .ReturnsAsync(usuarioCreado);

        // Act
        var result = await _controller.Verificar();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.username.Should().Be("test@test.com");

        _mockUsuarioService.Verify(s => s.Crear(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Verificar_ConUsuarioExistente_DeberiaActualizarYRetornarOk()
    {
        // Arrange
        var usuarioExistente = new Usuario 
        { 
            usuarioId = 1, 
            username = "test@test.com", 
            nombres = "Old Name", 
            apellidos = "Old Surname" 
        };

        var usuarioActualizado = new Usuario 
        { 
            usuarioId = 1, 
            username = "test@test.com", 
            nombres = "Test", 
            apellidos = "User" 
        };

        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync(usuarioExistente);
        _mockUsuarioService.Setup(s => s.Editar(It.IsAny<Usuario>()))
            .ReturnsAsync(usuarioActualizado);

        // Act
        var result = await _controller.Verificar();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.nombres.Should().Be("Test");
        response.apellidos.Should().Be("User");

        
        _mockUsuarioService.Verify(s => s.Editar(It.Is<Usuario>(u => 
                u.usuarioId == 1 &&
                u.username == "test@test.com"
        
        )), Times.Once);
    }

    #endregion
    
    #region Tests para EditarPerfil

    [Fact]
    public async Task EditarPerfil_ConUsuarioExistente_DeberiaRetornarOk()
    {
        // Arrange
        var usuarioExistente = new Usuario 
        { 
            usuarioId = 1, 
            username = "test@test.com", 
            nombres = "Old Name", 
            apellidos = "Old Surname",
            direccion = "Old Address",
            telefono = "Old Phone",
            telefono2 = "Old Phone2",
            comunaId = 1
        };

        var request = new UsuarioProfileRequest
        {
            nombres = "Updated Name",
            apellidos = "Updated Surname",
            direccion = "Updated Address",
            telefono = "Updated Phone",
            telefono2 = "Updated Phone2",
            comunaId = 2
        };

        var usuarioActualizado = new Usuario
        {
            usuarioId = 1,
            username = "test@test.com",
            nombres = request.nombres,
            apellidos = request.apellidos,
            direccion = request.direccion,
            telefono = request.telefono,
            telefono2 = request.telefono2,
            comunaId = request.comunaId
        };

        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync(usuarioExistente);
        _mockUsuarioService.Setup(s => s.Editar(It.IsAny<Usuario>()))
            .ReturnsAsync(usuarioActualizado);

        // Act
        var result = await _controller.EditarPerfil(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as UsuarioResponse;
        response.Should().NotBeNull();
        response.nombres.Should().Be(request.nombres);
        response.apellidos.Should().Be(request.apellidos);
        response.direccion.Should().Be(request.direccion);
        response.telefono.Should().Be(request.telefono);
        response.telefono2.Should().Be(request.telefono2);
        response.comuna?.comunaId.Should().Be(request.comunaId);
        response.rol.Should().Be("ADMIN");

        // Verificar que se actualizaron los campos correctos en el mismo objeto
        _mockUsuarioService.Verify(s => s.Editar(It.Is<Usuario>(u => 
            u.usuarioId == 1 &&
            u.username == "test@test.com" &&
            u.nombres == request.nombres &&
            u.apellidos == request.apellidos &&
            u.direccion == request.direccion &&
            u.telefono == request.telefono &&
            u.telefono2 == request.telefono2 &&
            u.comunaId == request.comunaId
        )), Times.Once);
    }

    [Fact]
    public async Task EditarPerfil_ConUsuarioInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var request = new UsuarioProfileRequest
        {
            nombres = "Test Name",
            apellidos = "Test Surname"
        };

        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.EditarPerfil(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Usuario no encontrado");

        // Verificar que no se intentó editar
        _mockUsuarioService.Verify(s => s.Editar(It.IsAny<Usuario>()), Times.Never);
    }

   

    [Fact]
    public async Task EditarPerfil_CuandoServicioLanzaExcepcion_DeberiaPropagarExcepcion()
    {
        // Arrange
        var usuarioExistente = new Usuario 
        { 
            usuarioId = 1, 
            username = "test@test.com", 
            nombres = "Test", 
            apellidos = "User" 
        };

        var request = new UsuarioProfileRequest
        {
            nombres = "Updated Name",
            apellidos = "Updated Surname"
        };

        _mockUsuarioService.Setup(s => s.GetByEmail("test@test.com"))
            .ReturnsAsync(usuarioExistente);
        _mockUsuarioService.Setup(s => s.Editar(It.IsAny<Usuario>()))
            .ThrowsAsync(new AppException("Error al actualizar el usuario"));

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _controller.EditarPerfil(request));
    }

    #endregion
}