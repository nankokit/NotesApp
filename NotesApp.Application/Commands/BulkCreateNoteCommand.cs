using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Commands;

public class BulkCreateNoteCommand : IRequest<List<NoteDto>>
{
    public required List<CreateNoteCommand> Notes { get; set; }
}