using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetTagByNameQueryHandler : IRequestHandler<GetTagByNameQuery, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagByNameQueryHandler> _logger;

    public GetTagByNameQueryHandler(ITagRepository tagRepository, ILogger<GetTagByNameQueryHandler> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagDto> Handle(GetTagByNameQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (tag == null)
        {
            _logger.LogWarning("Tag with name {TagName} not found", request.Name);
            throw new ResourceNotFoundException("Tag", request.Name);
        }

        var tagDto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };

        _logger.LogInformation("Successfully retrieved tag with name: {TagName}", tag.Name);

        return tagDto;
    }
}