using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Moq;
using PetsManagerMS.Controllers;
using PetsManagerMS.Dtos;
using PetsManagerMS.Services;
using Shared;

namespace PetsManagerMSTest;

public class OrganizacionControllerTests
{
    private readonly Mock<IOrganizacionService> _organizacionServiceMock;
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly OrganizacionController _controller;
    private readonly ClaimsPrincipal _user;
    
    public OrganizacionControllerTests()
    {
        _organizacionServiceMock = new Mock<IOrganizacionService>();
        _usuarioServiceMock = new Mock<IUsuarioService>();
        _controller = new OrganizacionController(_organizacionServiceMock.Object, _usuarioServiceMock.Object);
        
        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim("emails", "test@example.com"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _user = new ClaimsPrincipal(identity);
        
        // Setup controller context
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = _user }
        };
    }
    
    #region List Tests

    [Fact]
    public async Task List_SinUsuarioId_RetornaOkConListaCompleta()
    {
        // Arrange
        var organizaciones = new List<OrganizacionResponse>
        {
            new OrganizacionResponse ()
            {
                organizacionId = 1, 
                nombre = "Org1", 
                nombreContacto = "contacto", 
                direccion = "direccion", 
                emailContacto = "email", 
                telefonoContacto = "telefono",
                usuariosId = [],
                fechaEliminacion = null,
                comuna = new Comuna()
                {
                    nombre = "comuna",
                    comunaId = 1
                }
            },
            new OrganizacionResponse ()
            {
                organizacionId = 2, 
                nombre = "Org2", 
                nombreContacto = "contacto", 
                direccion = "direccion", 
                emailContacto = "email", 
                telefonoContacto = "telefono",
                usuariosId = [],
                fechaEliminacion = null,
                comuna = new Comuna()
                {
                    nombre = "comuna",
                    comunaId = 1
                }
            }
        };
        _organizacionServiceMock.Setup(s => s.List(null))
            .ReturnsAsync(organizaciones);

        // Act
        var result = await _controller.List(null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(organizaciones);
    }
    
    #endregion
    
    #region GetById Tests

    [Fact]
    public async Task GetById_OrganizacionExiste_RetornaOkConOrganizacion()
    {
        // Arrange
        var organizacionId = 1;
        var expectedOrganizacion = new OrganizacionResponse()
        {
            organizacionId = 1,
            nombre = "Org1",
            nombreContacto = "contacto",
            direccion = "direccion",
            emailContacto = "email",
            telefonoContacto = "telefono",
            usuariosId = [],
            fechaEliminacion = null,
            comuna = new Comuna()
            {
                nombre = "comuna",
                comunaId = 1
            }
        };
        _organizacionServiceMock.Setup(s => s.GetById(organizacionId))
            .ReturnsAsync(expectedOrganizacion);

        // Act
        var result = await _controller.GetById(organizacionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedOrganizacion);
    }

    [Fact]
    public async Task GetById_OrganizacionNoExiste_RetornaNotFound()
    {
        // Arrange
        var organizacionId = 999;
        _organizacionServiceMock.Setup(s => s.GetById(organizacionId))
            .ReturnsAsync((OrganizacionResponse?)null);

        // Act
        var result = await _controller.GetById(organizacionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Organizacion no encontrada");
    }

    #endregion
    
    #region Crear Tests

    [Fact]
    public async Task Crear_RequestValido_RetornaOkConOrganizacionCreada()
    {
        // Arrange
        var request = new OrganizacionRequest
        {
            nombre = "Org1",
            nombreContacto = "contacto",
            telefonoContacto = "telefono",
            emailContacto = "email",
            direccion = "direccion",
            comunaId = 1
        };
        var expectedResponse = new OrganizacionResponse()
        {
            organizacionId = 1,
            nombre = "Org1",
            nombreContacto = "contacto",
            direccion = "direccion",
            emailContacto = "email",
            telefonoContacto = "telefono",
            usuariosId = [],
            fechaEliminacion = null,
            comuna = new Comuna()
            {
                nombre = "comuna",
                comunaId = 1
            }
        };
        _organizacionServiceMock.Setup(s => s.Crear(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Crear(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    #endregion
    
    #region Editar Tests

    [Fact]
    public async Task Editar_RequestValido_RetornaOkConOrganizacionActualizada()
    {
        // Arrange
        var organizacionId = 1;
        var request = new OrganizacionRequest
        {
            nombre = "Org1",
            nombreContacto = "contacto",
            telefonoContacto = "telefono",
            emailContacto = "email",
            direccion = "direccion",
            comunaId = 1
        };
        var expectedResponse = new OrganizacionResponse()
        {
            organizacionId = 1,
            nombre = "Org1",
            nombreContacto = "contacto",
            direccion = "direccion",
            emailContacto = "email",
            telefonoContacto = "telefono",
            usuariosId = [],
            fechaEliminacion = null,
            comuna = new Comuna()
            {
                nombre = "comuna",
                comunaId = 1
            }
        };
        _organizacionServiceMock.Setup(s => s.Editar(organizacionId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Editar(organizacionId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    #endregion
    #region Eliminar Tests

    [Fact]
    public async Task Eliminar_OrganizacionExiste_RetornaNoContent()
    {
        // Arrange
        var organizacionId = 1;
        _organizacionServiceMock.Setup(s => s.Eliminar(organizacionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Eliminar(organizacionId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _organizacionServiceMock.Verify(s => s.Eliminar(organizacionId), Times.Once);
    }

    #endregion
    
    #region AgregarUsuario Tests

    [Fact]
    public async Task AgregarUsuario_ParametrosValidos_RetornaOk()
    {
        // Arrange
        var organizacionId = 1;
        var usuarioId = 1;
        _organizacionServiceMock.Setup(s => s.AgregarUsuario(organizacionId, usuarioId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AgregarUsuario(organizacionId, usuarioId);

        // Assert
        result.Should().BeOfType<OkResult>();
        _organizacionServiceMock.Verify(s => s.AgregarUsuario(organizacionId, usuarioId), Times.Once);
    }

    #endregion
    
    #region QuitarUsuario Tests

    [Fact]
    public async Task QuitarUsuario_ParametrosValidos_RetornaNoContent()
    {
        // Arrange
        var organizacionId = 1;
        var usuarioId = 1;
        _organizacionServiceMock.Setup(s => s.QuitarUsuario(organizacionId, usuarioId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.QuitarUsuario(organizacionId, usuarioId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _organizacionServiceMock.Verify(s => s.QuitarUsuario(organizacionId, usuarioId), Times.Once);
    }

    #endregion
    
    #region ListOrganizacionesOwner Tests

    [Fact]
    public async Task ListOrganizacionesOwner_UsuarioAutenticado_RetornaOkConOrganizaciones()
    {
        // Arrange
        var userEmail = "test@example.com";
        var usuario = new Usuario { usuarioId = 1, username = userEmail };
        var expectedOrganizaciones = new List<OrganizacionResponse>
        {
            new OrganizacionResponse()
            {
                organizacionId = 1,
                nombre = "Org1",
                nombreContacto = "contacto",
                direccion = "direccion",
                emailContacto = "email",
                telefonoContacto = "telefono",
                usuariosId = [1],
                fechaEliminacion = null,
                comuna = new Comuna()
                {
                    nombre = "comuna",
                    comunaId = 1
                }
            }
        };

        SetupControllerContext(userEmail);
        _usuarioServiceMock.Setup(s => s.VerificaUsuario(userEmail))
            .ReturnsAsync(usuario);
        _organizacionServiceMock.Setup(s => s.List(usuario.usuarioId))
            .ReturnsAsync(expectedOrganizaciones);

        // Act
        var result = await _controller.ListOrganizacionesOwner();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedOrganizaciones);
    }

    #endregion
    #region EditarOrganizacionOwner Tests

    [Fact]
    public async Task EditarOrganizacionOwner_UsuarioTienePermisos_RetornaOkConOrganizacionActualizada()
    {
        // Arrange
        var organizacionId = 1;
        var userEmail = "test@example.com";
        var request = new OrganizacionRequest
        {
            nombre = "Org1",
            nombreContacto = "contacto",
            direccion = "direccion",
            emailContacto = "email",
            telefonoContacto = "telefono",
            comunaId = 1
        };
        var expectedResponse = new OrganizacionResponse()
        {
            organizacionId = organizacionId,
            nombre = "Org1",
            nombreContacto = "contacto",
            direccion = "direccion",
            emailContacto = "email",
            telefonoContacto = "telefono",
            usuariosId = [1],
            fechaEliminacion = null,
            comuna = new Comuna()
            {
                nombre = "comuna",
                comunaId = 1
            }
        };

        SetupControllerContext(userEmail);
        _usuarioServiceMock.Setup(s => s.VerificaUsuarioOrganizacion(userEmail, organizacionId))
            .Returns(Task.CompletedTask);
        _organizacionServiceMock.Setup(s => s.Editar(organizacionId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.EditarOrganizacionOwner(organizacionId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
        _usuarioServiceMock.Verify(s => s.VerificaUsuarioOrganizacion(userEmail, organizacionId), Times.Once);
    }

    #endregion
    
    #region Exception Handling Tests

    [Fact]
    public async Task ListOrganizacionesOwner_UsuarioServiceThrowsException_PropagatesException()
    {
        // Arrange
        var userEmail = "test@example.com";
        SetupControllerContext(userEmail);
        _usuarioServiceMock.Setup(s => s.VerificaUsuario(userEmail))
            .ThrowsAsync(new AppException("Usuario no se encuentra registrado"));

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _controller.ListOrganizacionesOwner());
    }
    
    [Fact]
    public async Task EditarOrganizacionOwner_UsuarioSinPermisos_ThrowsException()
    {
        // Arrange
        var organizacionId = 1;
        var userEmail = "test@example.com";
        var request = new OrganizacionRequest
        {
            nombre = "Org1",
            nombreContacto = "contacto",
            direccion = "direccion",
            emailContacto = "email",
            telefonoContacto = "telefono",
            comunaId = 1
        };

        SetupControllerContext(userEmail);
        _usuarioServiceMock.Setup(s => s.VerificaUsuarioOrganizacion(userEmail, organizacionId))
            .ThrowsAsync(new AppException("Usuario no pertenece a la organizacion"));

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => 
            _controller.EditarOrganizacionOwner(organizacionId, request));
    }

    #endregion

    
    private void SetupControllerContext(string userEmail)
    {
        var claims = new List<Claim>
        {
            new Claim("emails", userEmail)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }
}