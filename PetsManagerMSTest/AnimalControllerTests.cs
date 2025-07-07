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

public class AnimalControllerTests
{
    private readonly Mock<IAnimalService> _mockAnimalService;
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly AnimalController _controller;
    
    public AnimalControllerTests()
    {
        _mockAnimalService = new Mock<IAnimalService>();
        _mockUsuarioService = new Mock<IUsuarioService>();
        _controller = new AnimalController(_mockAnimalService.Object, _mockUsuarioService.Object);
    }
    
    private void SetupSuperAdminUser()
    {
        var claims = new List<Claim>
        {
            new Claim("emails", "superadmin@gmail.com"),
            new Claim("extension_Roles", "SUPER_ADMIN")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }
    
    private void SetupAdminUser()
    {
        var claims = new List<Claim>
        {
            new Claim("emails", "admin@gmail.com"),
            new Claim("extension_Roles", "ADMIN")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }
    
    #region List Tests
    [Fact]
    public async Task List_ShouldReturnOkResult_WithAnimals()
    {
        // Arrange
        var query = new AnimalQuery { page = 1, pageSize = 10 };
        var expectedResult = new PageResult<AnimalResponse>
        {
            items = new List<AnimalResponse>
            {
                new AnimalResponse { animalId = 1, nombre = "Perro1" },
                new AnimalResponse { animalId = 2, nombre = "Gato1" }
            },
            totalCount = 2,
            page = 1,
            pageSize = 10
        };

        _mockAnimalService.Setup(s => s.List(query))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.List(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }
    #endregion
    
    #region GetById Tests

    [Fact]
    public async Task GetById_ShouldReturnOkResult_WhenAnimalExists()
    {
        // Arrange
        var animalId = 1;
        var expectedAnimal = new AnimalResponse { animalId = animalId, nombre = "Perro1" };

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(expectedAnimal);

        // Act
        var result = await _controller.GetById(animalId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedAnimal);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        // Arrange
        var animalId = 1;
        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync((AnimalResponse?)null);

        // Act
        var result = await _controller.GetById(animalId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Animal no encontrado");
    }

    #endregion
    
    #region Crear Tests

    [Fact]
    public async Task Crear_ShouldReturnOkResult_WhenUserIsSuperAdmin()
    {
        // Arrange
        var request = new AnimalRequest
        {
            nombre = "Nuevo Animal",
            peso = 10.5m,
            fechaNacimiento = DateTime.Now.AddYears(-2),
            publicado = true,
            descripcion = "Descripción",
            especieId = 1,
            sexoId = 1,
            organizacionId = 1,
            tamanoId = 1,
            nivelActividadId = 1
        };

        var expectedResponse = new AnimalResponse { animalId = 1, nombre = "Nuevo Animal" };

        SetupSuperAdminUser();

        _mockAnimalService.Setup(s => s.Crear(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Crear(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Crear_ShouldReturnOkResult_WhenUserIsNotSuperAdminButBelongsToOrganization()
    {
        // Arrange
        var request = new AnimalRequest
        {
            nombre = "Nuevo Animal",
            peso = 10.5m,
            fechaNacimiento = DateTime.Now.AddYears(-2),
            publicado = true,
            descripcion = "Descripción",
            especieId = 1,
            sexoId = 1,
            organizacionId = 1,
            tamanoId = 1,
            nivelActividadId = 1
        };

        var expectedResponse = new AnimalResponse { animalId = 1, nombre = "Nuevo Animal" };

        SetupAdminUser();

        _mockUsuarioService.Setup(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", request.organizacionId))
            .Returns(Task.CompletedTask);

        _mockAnimalService.Setup(s => s.Crear(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Crear(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResponse);
        _mockUsuarioService.Verify(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", request.organizacionId), Times.Once);
    }

    #endregion
    
    #region Editar Tests

    [Fact]
    public async Task Editar_ShouldReturnOkResult_WhenUserIsSuperAdmin()
    {
        // Arrange
        var animalId = 1;
        var request = new AnimalRequest
        {
            nombre = "Animal Editado",
            peso = 15.5m,
            fechaNacimiento = DateTime.Now.AddYears(-3),
            publicado = false,
            descripcion = "Descripción editada",
            especieId = 2,
            sexoId = 2,
            organizacionId = 1,
            tamanoId = 2,
            nivelActividadId = 2
        };

        var expectedResponse = new AnimalResponse { animalId = animalId, nombre = "Animal Editado" };

        SetupSuperAdminUser();

        _mockAnimalService.Setup(s => s.Editar(animalId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Editar(animalId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Editar_ShouldReturnOkResult_WhenUserIsNotSuperAdminButBelongsToOrganization()
    {
        // Arrange
        var animalId = 1;
        var request = new AnimalRequest
        {
            nombre = "Animal Editado",
            peso = 15.5m,
            fechaNacimiento = DateTime.Now.AddYears(-3),
            publicado = false,
            descripcion = "Descripción editada",
            especieId = 2,
            sexoId = 2,
            organizacionId = 1,
            tamanoId = 2,
            nivelActividadId = 2
        };

        var expectedResponse = new AnimalResponse { animalId = animalId, nombre = "Animal Editado" };

        SetupAdminUser();

        _mockUsuarioService.Setup(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", request.organizacionId))
            .Returns(Task.CompletedTask);

        _mockAnimalService.Setup(s => s.Editar(animalId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Editar(animalId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResponse);
        _mockUsuarioService.Verify(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", request.organizacionId), Times.Once);
    }

    #endregion
    
    #region Eliminar Tests

    [Fact]
    public async Task Eliminar_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        // Arrange
        var animalId = 1;
        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync((AnimalResponse?)null);

        // Act
        var result = await _controller.Eliminar(animalId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Animal no encontrado");
    }

    [Fact]
    public async Task Eliminar_ShouldReturnNoContent_WhenUserIsSuperAdmin()
    {
        // Arrange
        var animalId = 1;
        var animal = new AnimalResponse { animalId = animalId, organizacionId = 1 };

        SetupSuperAdminUser();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(animal);

        _mockAnimalService.Setup(s => s.Eliminar(animalId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Eliminar(animalId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockAnimalService.Verify(s => s.Eliminar(animalId), Times.Once);
    }

    [Fact]
    public async Task Eliminar_ShouldReturnNoContent_WhenUserIsNotSuperAdminButBelongsToOrganization()
    {
        // Arrange
        var animalId = 1;
        var animal = new AnimalResponse { animalId = animalId, organizacionId = 1 };

        SetupAdminUser();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(animal);

        _mockUsuarioService.Setup(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", animal.organizacionId))
            .Returns(Task.CompletedTask);

        _mockAnimalService.Setup(s => s.Eliminar(animalId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Eliminar(animalId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockUsuarioService.Verify(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", animal.organizacionId), Times.Once);
        _mockAnimalService.Verify(s => s.Eliminar(animalId), Times.Once);
    }

    #endregion
    
    #region Catalog Tests

    [Fact]
    public async Task ListarSexos_ShouldReturnOkResult_WithSexos()
    {
        // Arrange
        var expectedSexos = new List<Sexo> 
        { 
            new Sexo { sexoId = 1, nombre = "Macho" }, 
            new Sexo { sexoId = 2, nombre = "Hembra" } 
        };

        _mockAnimalService.Setup(s => s.ListarSexos())
            .ReturnsAsync(expectedSexos);

        // Act
        var result = await _controller.ListarSexos();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedSexos);
    }

    [Fact]
    public async Task ListarEspecies_ShouldReturnOkResult_WithEspecies()
    {
        // Arrange
        var expectedEspecies = new List<Especie> 
        { 
            new Especie { especieId = 1, nombre = "Perro" }, 
            new Especie { especieId = 2, nombre = "Gato" } 
        };

        _mockAnimalService.Setup(s => s.ListarEspecies())
            .ReturnsAsync(expectedEspecies);

        // Act
        var result = await _controller.ListarEspecies();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedEspecies);
    }

    [Fact]
    public async Task ListarTamanos_ShouldReturnOkResult_WithTamanos()
    {
        // Arrange
        var expectedTamanos = new List<Tamano> 
        { 
            new Tamano { tamanoId = 1, nombre = "Pequeño" }, 
            new Tamano { tamanoId = 2, nombre = "Grande" } 
        };

        _mockAnimalService.Setup(s => s.ListarTamanos())
            .ReturnsAsync(expectedTamanos);

        // Act
        var result = await _controller.ListarTamanos();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedTamanos);
    }

    [Fact]
    public async Task ListarNivelActividades_ShouldReturnOkResult_WithNivelActividades()
    {
        // Arrange
        var expectedNiveles = new List<NivelActividad> 
        { 
            new NivelActividad { nivelActividadId = 1, nombre = "Bajo" }, 
            new NivelActividad { nivelActividadId = 2, nombre = "Alto" } 
        };

        _mockAnimalService.Setup(s => s.ListarNivelActividades())
            .ReturnsAsync(expectedNiveles);

        // Act
        var result = await _controller.ListarNivelActividades();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedNiveles);
    }

    #endregion
    
    #region Image Tests
    [Fact]
    public async Task AgregarImagen_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        // Arrange
        var animalId = 1;
        var mockFile = new Mock<IFormFile>();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync((AnimalResponse?)null);

        // Act
        var result = await _controller.AgregarImagen(animalId, mockFile.Object);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Animal no encontrado");
    }
    [Fact]
    public async Task AgregarImagen_ShouldReturnNoContent_WhenUserIsSuperAdmin()
    {
        // Arrange
        var animalId = 1;
        var animal = new AnimalResponse { animalId = animalId, organizacionId = 1 };
        var mockFile = new Mock<IFormFile>();

        SetupSuperAdminUser();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(animal);

        _mockAnimalService.Setup(s => s.AgregarImagen(animalId, mockFile.Object))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AgregarImagen(animalId, mockFile.Object);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockAnimalService.Verify(s => s.AgregarImagen(animalId, mockFile.Object), Times.Once);
    }
    
    [Fact]
    public async Task AgregarImagen_ShouldReturnNoContent_WhenUserIsNotSuperAdminButBelongsToOrganization()
    {
        // Arrange
        var animalId = 1;
        var animal = new AnimalResponse { animalId = animalId, organizacionId = 1 };
        var mockFile = new Mock<IFormFile>();

        SetupAdminUser();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(animal);

        _mockUsuarioService.Setup(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", animal.organizacionId))
            .Returns(Task.CompletedTask);

        _mockAnimalService.Setup(s => s.AgregarImagen(animalId, mockFile.Object))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AgregarImagen(animalId, mockFile.Object);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockUsuarioService.Verify(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", animal.organizacionId), Times.Once);
        _mockAnimalService.Verify(s => s.AgregarImagen(animalId, mockFile.Object), Times.Once);
    }
    
    [Fact]
    public async Task EliminarImagen_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        // Arrange
        var animalId = 1;
        var imagenId = 1;

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync((AnimalResponse?)null);

        // Act
        var result = await _controller.EliminarImagen(animalId, imagenId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Animal no encontrado");
    }
    
    [Fact]
    public async Task EliminarImagen_ShouldReturnNoContent_WhenUserIsSuperAdmin()
    {
        // Arrange
        var animalId = 1;
        var imagenId = 1;
        var animal = new AnimalResponse { animalId = animalId, organizacionId = 1 };

        SetupSuperAdminUser();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(animal);

        _mockAnimalService.Setup(s => s.EliminarImagen(animalId, imagenId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EliminarImagen(animalId, imagenId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockAnimalService.Verify(s => s.EliminarImagen(animalId, imagenId), Times.Once);
    }
    
    [Fact]
    public async Task EliminarImagen_ShouldReturnNoContent_WhenUserIsNotSuperAdminButBelongsToOrganization()
    {
        // Arrange
        var animalId = 1;
        var imagenId = 1;
        var animal = new AnimalResponse { animalId = animalId, organizacionId = 1 };

        SetupAdminUser();

        _mockAnimalService.Setup(s => s.GetById(animalId))
            .ReturnsAsync(animal);

        _mockUsuarioService.Setup(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", animal.organizacionId))
            .Returns(Task.CompletedTask);

        _mockAnimalService.Setup(s => s.EliminarImagen(animalId, imagenId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EliminarImagen(animalId, imagenId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockUsuarioService.Verify(s => s.VerificaUsuarioOrganizacion("admin@gmail.com", animal.organizacionId), Times.Once);
        _mockAnimalService.Verify(s => s.EliminarImagen(animalId, imagenId), Times.Once);
    }
    #endregion
    
    #region ListAnimalesUsuario Tests
    [Fact]
    public async Task ListAnimalesUsuario_ShouldReturnOkResult_WithUserAnimals()
    {
        // Arrange
        var usuario = new Usuario() { usuarioId = 1, username = "admin@gmail.com", activo = true, nombres = "nombres"};
        var expectedAnimales = new List<AnimalResponse>
        {
            new AnimalResponse { animalId = 1, nombre = "Animal Usuario 1" },
            new AnimalResponse { animalId = 2, nombre = "Animal Usuario 2" }
        };

        SetupAdminUser();

        _mockUsuarioService.Setup(s => s.VerificaUsuario("admin@gmail.com"))
            .ReturnsAsync(usuario);

        _mockAnimalService.Setup(s => s.GetByUsuario(usuario.usuarioId))
            .ReturnsAsync(expectedAnimales);

        // Act
        var result = await _controller.ListAnimalesUsuario();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedAnimales);
    }
    #endregion
}