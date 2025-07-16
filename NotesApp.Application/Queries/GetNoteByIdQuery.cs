using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Queries;

public class GetNoteByIdQuery : IRequest<NoteDto>
{
    public Guid Id { get; set; }
}