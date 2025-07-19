using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class BulkCreateNoteCommandHandler : IRequestHandler<BulkCreateNoteCommand, List<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;

    public BulkCreateNoteCommandHandler(INoteRepository noteRepository, ITagRepository tagRepository)
    {
        _noteRepository = noteRepository;
        _tagRepository = tagRepository;
    }

    public async Task<List<NoteDto>> Handle(BulkCreateNoteCommand request, CancellationToken cancellationToken)
    {
        var noteDtos = new List<NoteDto>();
        var notesToAdd = new List<Note>();

        foreach (var noteCommand in request.Notes)
        {
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Name = noteCommand.Name,
                Description = noteCommand.Description,
                Tags = new List<Tag>(),
                ImageUrls = noteCommand.ImageUrls,
                CreationDate = DateTime.UtcNow
            };
            foreach (var tagName in noteCommand.TagNames ?? new List<string>())
            {
                var tag = await _tagRepository.GetByNameAsync(tagName, cancellationToken);
                if (tag == null)
                {
                    tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                    await _tagRepository.AddAsync(tag, cancellationToken);
                }
                note.Tags.Add(tag);
            }
            notesToAdd.Add(note);
            noteDtos.Add(new NoteDto
            {
                Id = note.Id,
                Name = note.Name,
                Description = note.Description,
                TagNames = note.Tags.Select(t => t.Name).ToList(),
                ImageUrls = note.ImageUrls,
                CreationDate = note.CreationDate
            });
        }
        foreach (var note in notesToAdd)
        {
            await _noteRepository.AddAsync(note, cancellationToken);
        }

        return noteDtos;
    }
}