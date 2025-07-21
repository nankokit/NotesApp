using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Exceptions;
using NotesApp.WebApi.Controllers;
using Xunit;

namespace NotesApp.WebApi.Tests.Controllers;

public class TagsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TagsController _controller;

    public TagsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TagsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPagedResult_WhenQueryIsValid()
    {
        // Arrange
        var pagedResult = new PagedResult<TagDto>
        {
            Items = new List<TagDto>
            {
                new TagDto { Id = Guid.NewGuid(), Name = "Tag1" }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllTagsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithTagDto_WhenTagExists()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var tagDto = new TagDto { Id = tagId, Name = "Tag1" };
        _mediatorMock.Setup(m => m.Send(It.Is<GetTagByIdQuery>(q => q.Id == tagId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDto);

        // Act
        var result = await _controller.GetById(tagId, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(tagDto);
    }

    [Fact]
    public async Task GetById_ShouldThrowResourceNotFound_WhenTagDoesNotExist()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTagByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Tag", tagId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.GetById(tagId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The Tag with identifier {tagId} was not found.");
    }

    [Fact]
    public async Task GetByName_ShouldReturnOkWithTagDto_WhenTagExists()
    {
        // Arrange
        var tagName = "Tag1";
        var tagDto = new TagDto { Id = Guid.NewGuid(), Name = tagName };
        _mediatorMock.Setup(m => m.Send(It.Is<GetTagByNameQuery>(q => q.Name == tagName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDto);

        // Act
        var result = await _controller.GetByName(tagName, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(tagDto);
    }

    [Fact]
    public async Task GetByName_ShouldThrowResourceNotFound_WhenTagDoesNotExist()
    {
        // Arrange
        var tagName = "NonExistentTag";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTagByNameQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Tag", tagName));

        // Act
        Func<Task> act = async () => await _controller.GetByName(tagName, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The Tag with identifier {tagName} was not found.");
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTagCommand { Name = "Tag1" };
        var tagDto = new TagDto { Id = Guid.NewGuid(), Name = command.Name };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTagCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDto);

        // Act
        var result = await _controller.Create(command, CancellationToken.None);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues!["id"].Should().Be(tagDto.Id);
        createdResult.Value.Should().BeEquivalentTo(tagDto);
    }

    [Fact]
    public async Task Create_ShouldThrowDuplicateResource_WhenTagAlreadyExists()
    {
        // Arrange
        var command = new CreateTagCommand { Name = "Tag1" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTagCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateResourceException("Tag", command.Name));

        // Act
        Func<Task> act = async () => await _controller.Create(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateResourceException>()
            .WithMessage($"The Tag with identifier {command.Name} already exists.");
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenCommandIsValid()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new UpdateTagCommand { Id = tagId, Name = "UpdatedTag" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTagCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(tagId, command, CancellationToken.None);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdsDoNotMatch()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new UpdateTagCommand { Id = Guid.NewGuid(), Name = "UpdatedTag" };

        // Act
        var result = await _controller.Update(tagId, command, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Route ID does not match command ID");
    }

    [Fact]
    public async Task Update_ShouldThrowDuplicateResource_WhenTagNameAlreadyExists()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new UpdateTagCommand { Id = tagId, Name = "ExistingTag" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTagCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateResourceException("Tag", command.Name));

        // Act
        Func<Task> act = async () => await _controller.Update(tagId, command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateResourceException>()
            .WithMessage($"The Tag with identifier {command.Name} already exists.");
    }

    [Fact]
    public async Task Update_ShouldThrowResourceNotFound_WhenTagDoesNotExist()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new UpdateTagCommand { Id = tagId, Name = "UpdatedTag" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTagCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Tag", tagId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.Update(tagId, command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The Tag with identifier {tagId} was not found.");
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenTagExistsAndNotInUse()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(tagId, CancellationToken.None);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task Delete_ShouldThrowResourceNotFound_WhenTagDoesNotExist()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Tag", tagId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.Delete(tagId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The Tag with identifier {tagId} was not found.");
    }

    [Fact]
    public async Task Delete_ShouldThrowResourceInUse_WhenTagIsAssociatedWithNotes()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var tagName = "Tag1";
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceInUseException("Tag", tagName, 2));

        // Act
        Func<Task> act = async () => await _controller.Delete(tagId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceInUseException>()
            .WithMessage($"Cannot delete Tag with identifier {tagName} because it is associated with 2 resource(s).");
    }
}