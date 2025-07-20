using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand>
{
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMinioService _minioService;

    public UpdateNoteCommandHandler(INoteRepository noteRepository, ITagRepository tagRepository, IMinioService minioService)
    {
        _noteRepository = noteRepository;
        _tagRepository = tagRepository;
        _minioService = minioService;
    }

    public async Task Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);

        if (note == null) throw new Exception("Note not found");

        note.Name = request.Name;
        note.Description = request.Description;

        note.ImageFileNames = request.ImageFileNames;

        note.Tags?.Clear();
        note.Tags = new List<Tag>();

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

        await _noteRepository.UpdateAsync(note, cancellationToken);
    }
}