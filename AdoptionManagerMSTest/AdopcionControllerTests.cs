using System.Security.Claims;
using AdoptionManagerMS.Controllers;
using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Moq;

namespace AdoptionManagerMSTest;

public class AdopcionControllerTests
{
    private readonly Mock<IAdopcionService> _adopcionServiceMock;
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly AdopcionController _controller;
    
    public AdopcionControllerTests()
    {
        _adopcionServiceMock = new Mock<IAdopcionService>();
        _usuarioServiceMock = new Mock<IUsuarioService>();
        _controller = new AdopcionController(_adopcionServiceMock.Object, _usuarioServiceMock.Object);
    }
    
    private void SetupUserClaims(string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim("emails", email),
            new Claim("extension_Roles", role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext()
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }
    
    #region List Tests
    [Fact]
    public async Task List_WhenUserIsSuperAdmin_ShouldReturnAllAdopciones()
    {
        // Arrange
        var query = new AdopcionQuery { organizacionId = 1 };
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var adopciones = new List<AdopcionResponse> { new AdopcionResponse { adopcionId = 1 } };

        SetupUserClaims("admin@test.com", "SUPER_ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.List(query))
            .ReturnsAsync(adopciones);

        // Act
        var result = await _controller.List(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopciones);
        _adopcionServiceMock.Verify(x => x.List(query), Times.Once);
    }
    
    [Fact]
    public async Task List_WhenUserIsRegularUser_ShouldFilterByUserId()
    {
        // Arrange
        var query = new AdopcionQuery { organizacionId = 1 };
        var usuario = new Usuario { usuarioId = 5, username = "user@test.com" };
        var adopciones = new List<AdopcionResponse> { new AdopcionResponse { adopcionId = 1 } };

        SetupUserClaims("user@test.com", "USER");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.List(It.Is<AdopcionQuery>(q => q.usuarioId == 5)))
            .ReturnsAsync(adopciones);

        // Act
        var result = await _controller.List(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopciones);
        query.usuarioId.Should().Be(5);
        _adopcionServiceMock.Verify(x => x.List(query), Times.Once);
    }
    
    [Fact]
    public async Task List_WhenUserIsAdmin_ShouldCallListByAdminColaborador()
    {
        // Arrange
        var query = new AdopcionQuery();
        var usuario = new Usuario { usuarioId = 3, username = "admin@test.com" };
        var adopciones = new List<AdopcionResponse> { new AdopcionResponse { adopcionId = 1 } };

        SetupUserClaims("admin@test.com", "ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.ListByAdminColaborador(3))
            .ReturnsAsync(adopciones);

        // Act
        var result = await _controller.List(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopciones);
        _adopcionServiceMock.Verify(x => x.ListByAdminColaborador(3), Times.Once);
    }
    
    [Fact]
    public async Task List_WhenUserHasNoValidRole_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new AdopcionQuery();
        var usuario = new Usuario { usuarioId = 1, username = "test@test.com" };

        SetupUserClaims("test@test.com", "InvalidRole");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("test@test.com"))
            .ReturnsAsync(usuario);

        // Act
        var result = await _controller.List(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resultList = okResult.Value.Should().BeOfType<List<string>>().Subject;
        resultList.Should().BeEmpty();
    }
    #endregion
    
    #region GetById Tests
    [Fact]
    public async Task GetById_WhenUserIsSuperAdmin_AndAdopcionExists_ShouldReturnAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var adopcion = new AdopcionResponse { adopcionId = adopcionId };

        SetupUserClaims("admin@test.com", "SUPER_ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.GetById(adopcionId))
            .ReturnsAsync(adopcion);

        // Act
        var result = await _controller.GetById(adopcionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcion);
    }
    
    [Fact]
    public async Task GetById_WhenUserIsSuperAdmin_AndAdopcionNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };

        SetupUserClaims("admin@test.com", "SUPER_ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.GetById(adopcionId))
            .ReturnsAsync((AdopcionResponse?)null);

        // Act
        var result = await _controller.GetById(adopcionId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Adopcion no encontrada");
    }
    
    [Fact]
    public async Task GetById_WhenUserIsRegularUser_AndAdopcionExists_ShouldReturnAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 5, username = "user@test.com" };
        var adopcion = new AdopcionResponse { adopcionId = adopcionId };

        SetupUserClaims("user@test.com", "USER");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.GetByIdAndUser(adopcionId, 5))
            .ReturnsAsync(adopcion);

        // Act
        var result = await _controller.GetById(adopcionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcion);
    }

    [Fact]
    public async Task GetById_WhenUserIsRegularUser_AndAdopcionNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 5, username = "user@test.com" };

        SetupUserClaims("user@test.com", "USER");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.GetByIdAndUser(adopcionId, 5))
            .ReturnsAsync((AdopcionResponse?)null);

        // Act
        var result = await _controller.GetById(adopcionId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Adopcion no encontrada");
    }
    #endregion
    
    #region Solicitar Tests
    [Fact]
    public async Task Solicitar_ShouldSetUsuarioIdAndCallService()
    {
        // Arrange
        var request = new AdopcionSolicitarRequest 
        { 
            animalId = 1, 
            descripcionFamilia = "Familia responsable" 
        };
        var usuario = new Usuario { usuarioId = 5, username = "user@test.com" };
        var adopcionResponse = new AdopcionResponse { adopcionId = 1 };

        SetupUserClaims("user@test.com", "USER");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _adopcionServiceMock.Setup(x => x.Solicitar(It.IsAny<AdopcionSolicitarRequest>()))
            .ReturnsAsync(adopcionResponse);

        // Act
        var result = await _controller.Solicitar(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcionResponse);
        request.usuarioId.Should().Be(5);
        _adopcionServiceMock.Verify(x => x.Solicitar(request), Times.Once);
    }
    #endregion
    
    #region Aprobar Tests

    [Fact]
    public async Task Aprobar_WhenUserIsSuperAdmin_ShouldNotVerifyAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var adopcionResponse = new AdopcionResponse { adopcionId = adopcionId };

        SetupUserClaims("admin@test.com", "SUPER_ADMIN");
        _adopcionServiceMock.Setup(x => x.Aprobar(adopcionId))
            .ReturnsAsync(adopcionResponse);

        // Act
        var result = await _controller.Aprobar(adopcionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcionResponse);
        _usuarioServiceMock.Verify(x => x.VerificaUsuario(It.IsAny<string>()), Times.Never);
        _usuarioServiceMock.Verify(x => x.VerificaAdopcionUsuario(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Aprobar_WhenUserIsNotSuperAdmin_ShouldVerifyAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 3, username = "admin@test.com" };
        var adopcionResponse = new AdopcionResponse { adopcionId = adopcionId };

        SetupUserClaims("admin@test.com", "ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _usuarioServiceMock.Setup(x => x.VerificaAdopcionUsuario(adopcionId, 3))
            .Returns(Task.CompletedTask);
        _adopcionServiceMock.Setup(x => x.Aprobar(adopcionId))
            .ReturnsAsync(adopcionResponse);

        // Act
        var result = await _controller.Aprobar(adopcionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcionResponse);
        _usuarioServiceMock.Verify(x => x.VerificaUsuario("admin@test.com"), Times.Once);
        _usuarioServiceMock.Verify(x => x.VerificaAdopcionUsuario(adopcionId, 3), Times.Once);
    }

    #endregion
    
    #region Rechazar Tests

    [Fact]
    public async Task Rechazar_WhenUserIsSuperAdmin_ShouldNotVerifyAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var adopcionResponse = new AdopcionResponse { adopcionId = adopcionId };

        SetupUserClaims("admin@test.com", "SUPER_ADMIN");
        _adopcionServiceMock.Setup(x => x.Rechazar(adopcionId))
            .ReturnsAsync(adopcionResponse);

        // Act
        var result = await _controller.Rechazar(adopcionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcionResponse);
        _usuarioServiceMock.Verify(x => x.VerificaUsuario(It.IsAny<string>()), Times.Never);
        _usuarioServiceMock.Verify(x => x.VerificaAdopcionUsuario(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Rechazar_WhenUserIsNotSuperAdmin_ShouldVerifyAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 3, username = "admin@test.com" };
        var adopcionResponse = new AdopcionResponse { adopcionId = adopcionId };

        SetupUserClaims("admin@test.com", "ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _usuarioServiceMock.Setup(x => x.VerificaAdopcionUsuario(adopcionId, 3))
            .Returns(Task.CompletedTask);
        _adopcionServiceMock.Setup(x => x.Rechazar(adopcionId))
            .ReturnsAsync(adopcionResponse);

        // Act
        var result = await _controller.Rechazar(adopcionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(adopcionResponse);
        _usuarioServiceMock.Verify(x => x.VerificaUsuario("admin@test.com"), Times.Once);
        _usuarioServiceMock.Verify(x => x.VerificaAdopcionUsuario(adopcionId, 3), Times.Once);
    }

    #endregion
    
    #region Delete Tests

    [Fact]
    public async Task Delete_WhenUserIsSuperAdmin_ShouldNotVerifyAdopcion()
    {
        // Arrange
        var adopcionId = 1;

        SetupUserClaims("admin@test.com", "SUPER_ADMIN");
        _adopcionServiceMock.Setup(x => x.Eliminar(adopcionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(adopcionId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _usuarioServiceMock.Verify(x => x.VerificaUsuario(It.IsAny<string>()), Times.Never);
        _usuarioServiceMock.Verify(x => x.VerificaAdopcionUsuario(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _adopcionServiceMock.Verify(x => x.Eliminar(adopcionId), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenUserIsNotSuperAdmin_ShouldVerifyAdopcion()
    {
        // Arrange
        var adopcionId = 1;
        var usuario = new Usuario { usuarioId = 3, username = "admin@test.com" };

        SetupUserClaims("admin@test.com", "ADMIN");
        _usuarioServiceMock.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _usuarioServiceMock.Setup(x => x.VerificaAdopcionUsuario(adopcionId, 3))
            .Returns(Task.CompletedTask);
        _adopcionServiceMock.Setup(x => x.Eliminar(adopcionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(adopcionId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _usuarioServiceMock.Verify(x => x.VerificaUsuario("admin@test.com"), Times.Once);
        _usuarioServiceMock.Verify(x => x.VerificaAdopcionUsuario(adopcionId, 3), Times.Once);
        _adopcionServiceMock.Verify(x => x.Eliminar(adopcionId), Times.Once);
    }

    #endregion

    #region ListEstados Tests

    [Fact]
    public async Task ListEstados_ShouldReturnAdopcionEstados()
    {
        // Arrange
        var estados = new List<AdopcionEstado> 
        { 
            new AdopcionEstado { adopcionEstadoId = 1, nombre = "Pendiente" },
            new AdopcionEstado { adopcionEstadoId = 2, nombre = "Aprobado" }
        };

        _adopcionServiceMock.Setup(x => x.ListAdopcionEstados())
            .ReturnsAsync(estados);

        // Act
        var result = await _controller.ListEstados();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(estados);
        _adopcionServiceMock.Verify(x => x.ListAdopcionEstados(), Times.Once);
    }

    #endregion
}