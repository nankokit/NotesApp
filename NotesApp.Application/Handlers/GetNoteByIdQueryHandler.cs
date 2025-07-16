using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto>
{
    private readonly INoteRepository _noteRepository;

    public GetNoteByIdQueryHandler(INoteRepository noteRepository) => _noteRepository = noteRepository;

    public async Task<NoteDto> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
        if (note == null) throw new Exception("Note not found");

        return new NoteDto
        {
            Id = note.Id,
            Name = note.Name,
            Description = note.Description,
            TagNames = note.Tags.Select(t => t.Name).ToList(),
            ImageUrls = note.ImageUrls,
            CreationDate = note.CreationDate
        };
    }

}