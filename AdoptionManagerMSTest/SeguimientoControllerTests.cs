using System.Security.Claims;
using AdoptionManagerMS.Controllers;
using AdoptionManagerMS.Dtos;
using AdoptionManagerMS.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Moq;
using Shared;

namespace AdoptionManagerMSTest;

public class SeguimientoControllerTests
{
    private readonly Mock<ISeguimientoService> _mockSeguimientoService;
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly SeguimientoController _controller;

    public SeguimientoControllerTests()
    {
        _mockSeguimientoService = new Mock<ISeguimientoService>();
        _mockUsuarioService = new Mock<IUsuarioService>();
        _controller = new SeguimientoController(_mockSeguimientoService.Object, _mockUsuarioService.Object);
    }
    
    private void SetupUserContext(string email, string role)
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
    public async Task GetSeguimientos_WhenUserIsSuperAdmin_ShouldReturnAllSeguimientos()
    {
        // Arrange
        var query = new SeguimientoQuery();
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var expectedSeguimientos = new List<SeguimientoResponse>
        {
            new SeguimientoResponse 
            {
                seguimientoId = 1,
                fechaInteraccion = DateTime.UtcNow,
                fechaCreacion = DateTime.UtcNow,
                descripcion = "Descripcion",
                observacion = "Observacion",
                fechaActualizacion = DateTime.UtcNow,
                fechaCierre = null,
                adopcionId = 1,
                animalId = 1,
                usuarioId = 1, 
                usuarioNombre = "admin@test.com",
                animalNombre = "animal nombre",
            },
            new SeguimientoResponse 
            {
                seguimientoId = 2,
                fechaInteraccion = DateTime.UtcNow,
                fechaCreacion = DateTime.UtcNow,
                descripcion = "Descripcion",
                observacion = "Observacion",
                fechaActualizacion = DateTime.UtcNow,
                fechaCierre = null,
                adopcionId = 1,
                animalId = 1,
                usuarioId = 2, 
                usuarioNombre = "admin2@test.com",
                animalNombre = "animal nombre",
            }
        };

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.List(query))
            .ReturnsAsync(expectedSeguimientos);

        // Act
        var result = await _controller.GetSeguimientos(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimientos = okResult.Value.Should().BeAssignableTo<List<SeguimientoResponse>>().Subject;
        seguimientos.Should().HaveCount(2);
        seguimientos.Should().BeEquivalentTo(expectedSeguimientos);
    }
    
    [Fact]
    public async Task GetSeguimientos_WhenUserIsRegularUser_ShouldReturnUserSeguimientos()
    {
        // Arrange
        var query = new SeguimientoQuery();
        var usuario = new Usuario { usuarioId = 1, username = "user@test.com" };
        var expectedSeguimientos = new List<SeguimientoResponse>
        {
            new SeguimientoResponse 
            {
                seguimientoId = 1,
                fechaInteraccion = DateTime.UtcNow,
                fechaCreacion = DateTime.UtcNow,
                descripcion = "Descripcion",
                observacion = "Observacion",
                fechaActualizacion = DateTime.UtcNow,
                fechaCierre = null,
                adopcionId = 1,
                animalId = 1,
                usuarioId = 1, 
                usuarioNombre = "user@user.com",
                animalNombre = "animal nombre",
            }
        };

        SetupUserContext("user@test.com", "USER");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.List(It.Is<SeguimientoQuery>(q => q.usuarioId == 1)))
            .ReturnsAsync(expectedSeguimientos);

        // Act
        var result = await _controller.GetSeguimientos(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimientos = okResult.Value.Should().BeAssignableTo<List<SeguimientoResponse>>().Subject;
        seguimientos.Should().HaveCount(1);
        seguimientos.Should().BeEquivalentTo(expectedSeguimientos);
    }
    
    [Fact]
    public async Task GetSeguimientos_WhenUserIsAdmin_ShouldReturnAdminSeguimientos()
    {
        // Arrange
        var query = new SeguimientoQuery();
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var expectedSeguimientos = new List<SeguimientoResponse>
        {
            new SeguimientoResponse 
            {
                seguimientoId = 1,
                fechaInteraccion = DateTime.UtcNow,
                fechaCreacion = DateTime.UtcNow,
                descripcion = "Descripcion",
                observacion = "Observacion",
                fechaActualizacion = DateTime.UtcNow,
                fechaCierre = null,
                adopcionId = 1,
                animalId = 1,
                usuarioId = 1, 
                usuarioNombre = "admin@admin.com",
                animalNombre = "animal nombre",
            }
        };

        SetupUserContext("admin@test.com", "ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.ListByAdminColaborador(1))
            .ReturnsAsync(expectedSeguimientos);

        // Act
        var result = await _controller.GetSeguimientos(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimientos = okResult.Value.Should().BeAssignableTo<List<SeguimientoResponse>>().Subject;
        seguimientos.Should().HaveCount(1);
        seguimientos.Should().BeEquivalentTo(expectedSeguimientos);
    }
    
    [Fact]
    public async Task GetSeguimientos_WhenUserIsColaborator_ShouldReturnColaboratorSeguimientos()
    {
        // Arrange
        var query = new SeguimientoQuery();
        var usuario = new Usuario { usuarioId = 1, username = "colaborator@test.com" };
        var expectedSeguimientos = new List<SeguimientoResponse>
        {
            new SeguimientoResponse 
            {
                seguimientoId = 1,
                fechaInteraccion = DateTime.UtcNow,
                fechaCreacion = DateTime.UtcNow,
                descripcion = "Descripcion",
                observacion = "Observacion",
                fechaActualizacion = DateTime.UtcNow,
                fechaCierre = null,
                adopcionId = 1,
                animalId = 1,
                usuarioId = 1, 
                usuarioNombre = "colaborator@colaborator.com",
                animalNombre = "animal nombre",
            }
        };

        SetupUserContext("colaborator@test.com", "COLABORATOR");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("colaborator@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.ListByAdminColaborador(1))
            .ReturnsAsync(expectedSeguimientos);

        // Act
        var result = await _controller.GetSeguimientos(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimientos = okResult.Value.Should().BeAssignableTo<List<SeguimientoResponse>>().Subject;
        seguimientos.Should().HaveCount(1);
        seguimientos.Should().BeEquivalentTo(expectedSeguimientos);
    }
    #endregion
    
    #region GetById Tests
    [Fact]
    public async Task GetSeguimiento_WhenSuperAdminAndSeguimientoExists_ShouldReturnSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var expectedSeguimiento = new SeguimientoResponse 
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "Descripcion",
            observacion = "Observacion",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 2, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetById(seguimientoId))
            .ReturnsAsync(expectedSeguimiento);

        // Act
        var result = await _controller.GetSeguimiento(seguimientoId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimiento = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimiento.Should().BeEquivalentTo(expectedSeguimiento);
    }
    
    [Fact]
    public async Task GetSeguimiento_WhenSuperAdminAndSeguimientoNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var seguimientoId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetById(seguimientoId))
            .ReturnsAsync((SeguimientoResponse?)null);

        // Act
        var result = await _controller.GetSeguimiento(seguimientoId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Seguimiento no encontrado");
    }
    
    [Fact]
    public async Task GetSeguimiento_WhenUserAndSeguimientoExists_ShouldReturnSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "user@test.com" };
        var expectedSeguimiento = new SeguimientoResponse 
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "Descripcion",
            observacion = "Observacion",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("user@test.com", "USER");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetByIdAndUser(seguimientoId, usuario.usuarioId))
            .ReturnsAsync(expectedSeguimiento);

        // Act
        var result = await _controller.GetSeguimiento(seguimientoId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimiento = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimiento.Should().BeEquivalentTo(expectedSeguimiento);
    }
    
    [Fact]
    public async Task GetSeguimiento_WhenUserAndSeguimientoNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var seguimientoId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "user@test.com" };

        SetupUserContext("user@test.com", "User");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("user@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetByIdAndUser(seguimientoId, usuario.usuarioId))
            .ReturnsAsync((SeguimientoResponse?)null);

        // Act
        var result = await _controller.GetSeguimiento(seguimientoId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Seguimiento no encontrado");
    }
    
    #endregion
    
    #region Crear Tests
    [Fact]
    public async Task CrearSeguimiento_WhenSuperAdmin_ShouldCreateSeguimiento()
    {
        // Arrange
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "",
        };
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var expectedResponse = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = request.fechaInteraccion,
            fechaCreacion = DateTime.UtcNow,
            descripcion = request.descripcion,
            observacion = "Observacion",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.Crear(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CrearSeguimiento(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimiento = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimiento.Should().BeEquivalentTo(expectedResponse);
        _mockUsuarioService.Verify(x => x.VerificaAdopcionUsuario(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CrearSeguimiento_WhenAdminOrColaborator_ShouldVerifyAdopcionAndCreateSeguimiento()
    {
        // Arrange
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "",
        };
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var expectedResponse = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = request.fechaInteraccion,
            fechaCreacion = DateTime.UtcNow,
            descripcion = request.descripcion,
            observacion = "Observacion",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "Admin");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockUsuarioService.Setup(x => x.VerificaAdopcionUsuario(request.adopcionId, usuario.usuarioId))
            .Returns(Task.CompletedTask);
        _mockSeguimientoService.Setup(x => x.Crear(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CrearSeguimiento(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimiento = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimiento.Should().BeEquivalentTo(expectedResponse);
        _mockUsuarioService.Verify(x => x.VerificaAdopcionUsuario(request.adopcionId, usuario.usuarioId), Times.Once);
    }
    #endregion
    
    #region Editar Tests
    [Fact]
    public async Task EditarSeguimiento_WhenSuperAdmin_ShouldEditSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "",
        };
        var expectedResponse = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = request.fechaInteraccion,
            fechaCreacion = DateTime.UtcNow,
            descripcion = request.descripcion,
            observacion = "Observacion",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockSeguimientoService.Setup(x => x.Editar(seguimientoId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.EditarSeguimiento(seguimientoId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimiento = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimiento.Should().BeEquivalentTo(expectedResponse);
    }
    
    [Fact]
    public async Task EditarSeguimiento_WhenAdminOrColaborator_ShouldVerifyAndEditSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "",
        };
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var seguimiento = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "original",
            observacion = "original",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };
        var expectedResponse = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = request.fechaInteraccion,
            fechaCreacion = DateTime.UtcNow,
            descripcion = request.descripcion,
            observacion = "Observacion",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetById(seguimientoId))
            .ReturnsAsync(seguimiento);
        _mockUsuarioService.Setup(x => x.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId))
            .Returns(Task.CompletedTask);
        _mockSeguimientoService.Setup(x => x.Editar(seguimientoId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.EditarSeguimiento(seguimientoId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimientoResult = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimientoResult.Should().BeEquivalentTo(expectedResponse);
        _mockUsuarioService.Verify(x => x.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId), Times.Once);
    }
    #endregion
    
    #region Eliminar Tests
    [Fact]
    public async Task EliminarSeguimiento_WhenSuperAdmin_ShouldDeleteSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockSeguimientoService.Setup(x => x.Eliminar(seguimientoId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EliminarSeguimiento(seguimientoId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockSeguimientoService.Verify(x => x.Eliminar(seguimientoId), Times.Once);
    }

    [Fact]
    public async Task EliminarSeguimiento_WhenAdminOrColaborator_ShouldVerifyAndDeleteSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var seguimiento = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "original",
            observacion = "original",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "Admin");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetById(seguimientoId))
            .ReturnsAsync(seguimiento);
        _mockUsuarioService.Setup(x => x.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId))
            .Returns(Task.CompletedTask);
        _mockSeguimientoService.Setup(x => x.Eliminar(seguimientoId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EliminarSeguimiento(seguimientoId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockSeguimientoService.Verify(x => x.Eliminar(seguimientoId), Times.Once);
        _mockUsuarioService.Verify(x => x.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId), Times.Once);
    }
    #endregion
    
    #region Cerrar Tests
    [Fact]
    public async Task CerrarSeguimiento_WhenSuperAdmin_ShouldCloseSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var request = new SeguimientoCerrarRequest { observacion = "Cerrado por finalización" };
        var expectedResponse = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "original",
            observacion = "original",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "SUPER_ADMIN");
        _mockSeguimientoService.Setup(x => x.Cerrar(seguimientoId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CerrarSeguimiento(seguimientoId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimiento = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimiento.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CerrarSeguimiento_WhenAdminOrColaborator_ShouldVerifyAndCloseSeguimiento()
    {
        // Arrange
        var seguimientoId = 1;
        var request = new SeguimientoCerrarRequest { observacion = "Cerrado por finalización" };
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var seguimiento = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "original",
            observacion = "original",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };
        var expectedResponse = new SeguimientoResponse
        {
            seguimientoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            fechaCreacion = DateTime.UtcNow,
            descripcion = "original",
            observacion = "Cerrado por finalización",
            fechaActualizacion = DateTime.UtcNow,
            fechaCierre = null,
            adopcionId = 1,
            animalId = 1,
            usuarioId = 1, 
            usuarioNombre = "user@user.com",
            animalNombre = "animal nombre",
        };

        SetupUserContext("admin@test.com", "ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetById(seguimientoId))
            .ReturnsAsync(seguimiento);
        _mockUsuarioService.Setup(x => x.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId))
            .Returns(Task.CompletedTask);
        _mockSeguimientoService.Setup(x => x.Cerrar(seguimientoId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CerrarSeguimiento(seguimientoId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var seguimientoResult = okResult.Value.Should().BeAssignableTo<SeguimientoResponse>().Subject;
        seguimientoResult.Should().BeEquivalentTo(expectedResponse);
        _mockUsuarioService.Verify(x => x.VerificaAdopcionUsuario(seguimiento.adopcionId, usuario.usuarioId), Times.Once);
    }
    #endregion
    
    #region Datos Tests
    [Fact]
    public async Task GetEstados_ShouldReturnEstados()
    {
        // Arrange
        var expectedEstados = new List<SeguimientoEstado>
        {
            new SeguimientoEstado { seguimientoEstadoId = 1, nombre = "Abierto" },
            new SeguimientoEstado { seguimientoEstadoId = 2, nombre = "Cerrado" }
        };

        _mockSeguimientoService.Setup(x => x.GetEstados())
            .ReturnsAsync(expectedEstados);

        // Act
        var result = await _controller.GetEstados();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var estados = okResult.Value.Should().BeAssignableTo<List<SeguimientoEstado>>().Subject;
        estados.Should().BeEquivalentTo(expectedEstados);
    }
    
    [Fact]
    public async Task GetTipos_ShouldReturnTipos()
    {
        // Arrange
        var expectedTipos = new List<SeguimientoTipo>
        {
            new SeguimientoTipo { seguimientoTipoId = 1, nombre = "Visita" },
            new SeguimientoTipo { seguimientoTipoId = 2, nombre = "Llamada" }
        };

        _mockSeguimientoService.Setup(x => x.GetTipos())
            .ReturnsAsync(expectedTipos);

        // Act
        var result = await _controller.GetTipos();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var tipos = okResult.Value.Should().BeAssignableTo<List<SeguimientoTipo>>().Subject;
        tipos.Should().BeEquivalentTo(expectedTipos);
    }
    #endregion
    
    #region VerificaSeguimiento Tests

    [Fact]
    public async Task VerificaSeguimiento_WhenSeguimientoNotFound_ShouldThrowException()
    {
        // Arrange
        var seguimientoId = 1;
        var usuario = new Usuario { usuarioId = 1, username = "admin@test.com" };
        var request = new SeguimientoRequest
        {
            adopcionId = 1,
            seguimientoTipoId = 1,
            fechaInteraccion = DateTime.UtcNow,
            descripcion = "",
        };

        SetupUserContext("admin@test.com", "ADMIN");
        _mockUsuarioService.Setup(x => x.VerificaUsuario("admin@test.com"))
            .ReturnsAsync(usuario);
        _mockSeguimientoService.Setup(x => x.GetById(seguimientoId))
            .ReturnsAsync((SeguimientoResponse?)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _controller.EditarSeguimiento(seguimientoId, request));
    }

    #endregion
}