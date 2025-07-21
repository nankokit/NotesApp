using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<CreateTagCommandHandler> _logger;

    public CreateTagCommandHandler(ITagRepository tagRepository, ILogger<CreateTagCommandHandler> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null)
        {
            _logger.LogError("Tag with name {TagName} already exists", request.Name);
            throw new DuplicateResourceException("Tag", request.Name);
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        await _tagRepository.AddAsync(tag, cancellationToken);
        _logger.LogInformation("Successfully created tag with ID: {TagId}, Name: {TagName}", tag.Id, tag.Name);

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}