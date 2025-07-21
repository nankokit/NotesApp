using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Exceptions;
using NotesApp.WebApi.Controllers;
using Xunit;

namespace NotesApp.WebApi.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Register_ShouldReturnOkWithUserDto_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterCommand { Username = "testuser", Password = "password123" };
        var userDto = new UserDto { Id = Guid.NewGuid(), Username = command.Username };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.Register(command);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task Register_ShouldThrowDuplicateResource_WhenUsernameExists()
    {
        // Arrange
        var command = new RegisterCommand { Username = "existinguser", Password = "password123" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateResourceException("User", command.Username));

        // Act
        Func<Task> act = async () => await _controller.Register(command);

        // Assert
        await act.Should().ThrowAsync<DuplicateResourceException>()
            .WithMessage($"The User with identifier {command.Username} already exists.");
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginCommand { Username = "testuser", Password = "password123" };
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((accessToken, refreshToken));

        // Act
        var result = await _controller.Login(command);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedAccess_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = new LoginCommand { Username = "testuser", Password = "wrongpassword" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Domain.Exceptions.UnauthorizedAccessException("Invalid username or password"));

        // Act
        Func<Task> act = async () => await _controller.Login(command);

        // Assert
        await act.Should().ThrowAsync<Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Invalid username or password");
    }

    [Fact]
    public async Task Login_ShouldThrowConfigurationException_WhenJwtConfigIsMissing()
    {
        // Arrange
        var command = new LoginCommand { Username = "testuser", Password = "password123" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConfigurationException("JWT configuration is missing"));

        // Act
        Func<Task> act = async () => await _controller.Login(command);

        // Assert
        await act.Should().ThrowAsync<ConfigurationException>()
            .WithMessage("JWT configuration is missing");
    }

    [Fact]
    public async Task Refresh_ShouldReturnOkWithAccessToken_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
        var accessToken = "new-access-token";
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        // Act
        var result = await _controller.Refresh(command);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(new { AccessToken = accessToken });
    }

    [Fact]
    public async Task Refresh_ShouldThrowUnauthorizedAccess_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "invalid-refresh-token" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Domain.Exceptions.UnauthorizedAccessException("Invalid or expired refresh token"));

        // Act
        Func<Task> act = async () => await _controller.Refresh(command);

        // Assert
        await act.Should().ThrowAsync<Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Refresh_ShouldThrowConfigurationException_WhenJwtConfigIsMissing()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConfigurationException("JWT configuration is missing"));

        // Act
        Func<Task> act = async () => await _controller.Refresh(command);

        // Assert
        await act.Should().ThrowAsync<ConfigurationException>()
            .WithMessage("JWT configuration is missing");
    }

    [Fact]
    public async Task GetUser_ShouldReturnOkWithUserDto_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto { Id = userId, Username = "testuser" };
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUser();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetUser_ShouldThrowResourceNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("User", userId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.GetUser();

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The User with identifier {userId} was not found.");
    }

    [Fact]
    public async Task GetUser_ShouldThrowResourceNotFound_WhenUserIdIsInvalid()
    {
        // Arrange
        var invalidUserId = Guid.Empty;
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
            }, "mock"));
        _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Id == invalidUserId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("User", invalidUserId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.GetUser();

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The User with identifier {invalidUserId} was not found.");
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete();

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task Delete_ShouldThrowResourceNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("User", userId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.Delete();

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The User with identifier {userId} was not found.");
    }

    [Fact]
    public async Task Delete_ShouldThrowResourceNotFound_WhenUserIdIsInvalid()
    {
        // Arrange
        var invalidUserId = Guid.Empty;
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
            }, "mock"));
        _mediatorMock.Setup(m => m.Send(It.Is<DeleteUserCommand>(c => c.Id == invalidUserId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("User", invalidUserId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.Delete();

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The User with identifier {invalidUserId} was not found.");
    }
}