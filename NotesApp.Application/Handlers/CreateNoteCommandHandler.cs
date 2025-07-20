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
    private readonly IMinioService _minioService;

    public CreateNoteCommandHandler(INoteRepository noteRepository, ITagRepository tagRepository, IMinioService minioService)
    {
        _noteRepository = noteRepository;
        _tagRepository = tagRepository;
        _minioService = minioService;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Tags = new List<Tag>(),
            ImageFileNames = request.ImageFileNames,
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

        var imageUrls = new List<string>();
        if (request.ImageFileNames != null)
        {
            foreach (var fileName in request.ImageFileNames)
            {
                var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
                imageUrls.Add(url);
            }
        }

        await _noteRepository.AddAsync(note, cancellationToken);

        return new NoteDto
        {
            Id = note.Id,
            Name = note.Name,
            Description = note.Description,
            TagNames = note.Tags.Select(t => t.Name).ToList(),
            ImageUrls = imageUrls,
            CreationDate = note.CreationDate
        };

    }

}