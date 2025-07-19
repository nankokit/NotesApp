using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NotesApp.Application.Handlers;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand>
{
    private readonly ITagRepository _tagRepository;
    private readonly INoteRepository _noteRepository;

    public UpdateTagCommandHandler(ITagRepository tagRepository, INoteRepository noteRepository)
    {
        _tagRepository = tagRepository;
        _noteRepository = noteRepository;
    }

    public async Task Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with ID '{request.Id}' not found");
        }

        var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"Tag with name '{request.Name}' already exists");
        }

        tag.Name = request.Name;
        await _tagRepository.UpdateAsync(tag, cancellationToken);

        var notes = await _noteRepository.GetAllAsync(null, new List<string> { request.Name }, null, true, 1, int.MaxValue, cancellationToken);
        foreach (var note in notes)
        {
            if (note.Tags != null)
            {
                var tagToUpdate = note.Tags.First(t => t.Name == request.Name);
                tagToUpdate.Name = request.Name;
                await _noteRepository.UpdateAsync(note, cancellationToken);
            }
        }
    }
}