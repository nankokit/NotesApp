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
    IMinioService _minioService;

    public BulkCreateNoteCommandHandler(INoteRepository noteRepository, ITagRepository tagRepository, IMinioService minioService)
    {
        _noteRepository = noteRepository;
        _tagRepository = tagRepository;
        _minioService = minioService;
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
                ImageFileNames = noteCommand.ImageFileNames,
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

            var imageUrls = new List<string>();
            if (noteCommand.ImageFileNames != null)
            {
                foreach (var fileName in noteCommand.ImageFileNames)
                {
                    var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
                    imageUrls.Add(url);
                }
            }

            notesToAdd.Add(note);

            noteDtos.Add(new NoteDto
            {
                Id = note.Id,
                Name = note.Name,
                Description = note.Description,
                TagNames = note.Tags.Select(t => t.Name).ToList(),
                ImageUrls = imageUrls,
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