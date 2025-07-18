using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;

    public CreateNoteCommandHandler(INoteRepository noteRepository, ITagRepository tagRepository)
    {
        _noteRepository = noteRepository;
        _tagRepository = tagRepository;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ImageUrls = request.ImageUrls,
            CreationDate = DateTime.UtcNow
        };
        foreach (var tagName in request.TagNames ?? new List<string>())
        {
            var tag = await _tagRepository.GetByNameAsync(tagName, cancellationToken);
            if (tag == null)
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                await _tagRepository.AddAsync(tag, cancellationToken);
            }
            note.Tags.Add(tag);
        }
        await _noteRepository.AddAsync(note, cancellationToken);
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