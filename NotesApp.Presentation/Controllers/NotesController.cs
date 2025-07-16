using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;

namespace NotesApp.Presentation.Controllers;

public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetAll([FromQuery] GetAllNotesQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetNoteByIdQuery { Id = id };
        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create([FromBody] CreateNoteCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != command.Id)
        {
            return BadRequest("Route ID does not match command ID");
        }

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch
        {
            return NotFound();
        }
    }

}