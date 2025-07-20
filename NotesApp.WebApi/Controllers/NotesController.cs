using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMinioService _minioService;

    public NotesController(IMediator mediator, IMinioService minioService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<NoteDto>>> Search([FromBody] GetAllNotesQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetNoteByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("image/{fileName}")]
    public async Task<IActionResult> GetImage(string fileName, CancellationToken cancellationToken)
    {
        var stream = await _minioService.GetImageAsync(fileName, cancellationToken);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        string contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return File(stream, contentType);
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create([FromBody] CreateNoteCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<List<NoteDto>>> BulkCreate([FromBody] BulkCreateNoteCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(Search), null, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match command ID");
        }

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteNoteCommand { Id = id }, cancellationToken);

        return NoContent();
    }
}