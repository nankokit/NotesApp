using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;

    public CreateTagCommandHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"Tag with name '{request.Name}' already exists");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        await _tagRepository.AddAsync(tag, cancellationToken);
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}