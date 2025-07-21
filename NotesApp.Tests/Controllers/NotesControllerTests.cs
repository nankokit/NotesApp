using System;
using System.Collections.Generic;
using System.IO;
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
using NotesApp.Domain.Interfaces;
using NotesApp.WebApi.Controllers;
using Xunit;

namespace NotesApp.Tests.Controllers;

public class NotesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMinioService> _minioServiceMock;
    private readonly NotesController _controller;

    public NotesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _minioServiceMock = new Mock<IMinioService>();
        _controller = new NotesController(_mediatorMock.Object, _minioServiceMock.Object);
    }

    [Fact]
    public async Task Search_ShouldReturnOkWithPagedResult_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetAllNotesQuery();
        var pagedResult = new PagedResult<NoteDto>
        {
            Items = new List<NoteDto>
            {
                new NoteDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Note",
                    Description = "Test Description",
                    CreationDate = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllNotesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.Search(query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithNoteDto_WhenNoteExists()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var noteDto = new NoteDto
        {
            Id = noteId,
            Name = "Test Note",
            Description = "Test Description",
            CreationDate = DateTime.UtcNow
        };
        _mediatorMock.Setup(m => m.Send(It.Is<GetNoteByIdQuery>(q => q.Id == noteId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(noteDto);

        // Act
        var result = await _controller.GetById(noteId, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(noteDto);
    }

    [Fact]
    public async Task GetById_ShouldThrowResourceNotFound_WhenNoteDoesNotExist()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNoteByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Note", noteId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.GetById(noteId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The Note with identifier {noteId} was not found.");
    }

    [Fact]
    public async Task GetImage_ShouldReturnFileResult_WhenImageExists()
    {
        // Arrange
        var fileName = "test.jpg";
        var stream = new MemoryStream();
        _minioServiceMock.Setup(m => m.GetImageAsync(fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        // Act
        var result = await _controller.GetImage(fileName, CancellationToken.None);

        // Assert
        var fileResult = result as FileStreamResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("image/jpeg");
        fileResult.FileStream.Should().BeSameAs(stream);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateNoteCommand
        {
            Name = "Test Note",
            Description = "Test Description",
            TagNames = new List<string> { "Tag1" },
            ImageFileNames = new List<string> { "image.jpg" }
        };
        var noteDto = new NoteDto
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            TagNames = command.TagNames,
            ImageUrls = new List<string> { "http://example.com/image.jpg" },
            CreationDate = DateTime.UtcNow
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(noteDto);

        // Act
        var result = await _controller.Create(command, CancellationToken.None);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues!["id"].Should().Be(noteDto.Id);
        createdResult.Value.Should().BeEquivalentTo(noteDto);
    }

    [Fact]
    public async Task BulkCreate_ShouldReturnCreatedAtAction_WhenCommandIsValid()
    {
        // Arrange
        var command = new BulkCreateNoteCommand
        {
            Notes = new List<CreateNoteCommand>
            {
                new CreateNoteCommand
                {
                    Name = "Test Note",
                    Description = "Test Description",
                    TagNames = new List<string> { "Tag1" }
                }
            }
        };
        var noteDtos = new List<NoteDto>
        {
            new NoteDto
            {
                Id = Guid.NewGuid(),
                Name = "Test Note",
                Description = "Test Description",
                TagNames = new List<string> { "Tag1" },
                CreationDate = DateTime.UtcNow
            }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<BulkCreateNoteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(noteDtos);

        // Act
        var result = await _controller.BulkCreate(command, CancellationToken.None);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(_controller.Search));
        createdResult.Value.Should().BeEquivalentTo(noteDtos);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenCommandIsValid()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var command = new UpdateNoteCommand
        {
            Id = noteId,
            Name = "Updated Note",
            Description = "Updated Description"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateNoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(noteId, command, CancellationToken.None);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdsDoNotMatch()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var command = new UpdateNoteCommand
        {
            Id = Guid.NewGuid(), // Different ID
            Name = "Updated Note",
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.Update(noteId, command, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Route ID does not match command ID");
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenNoteExists()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteNoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(noteId, CancellationToken.None);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task Delete_ShouldThrowResourceNotFound_WhenNoteDoesNotExist()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteNoteCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Note", noteId.ToString()));

        // Act
        Func<Task> act = async () => await _controller.Delete(noteId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"The Note with identifier {noteId} was not found.");
    }
}