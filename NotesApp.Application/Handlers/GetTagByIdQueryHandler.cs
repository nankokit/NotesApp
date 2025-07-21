using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagByIdQueryHandler> _logger;

    public GetTagByIdQueryHandler(ITagRepository tagRepository, ILogger<GetTagByIdQueryHandler> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagDto> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            _logger.LogWarning("Tag with ID {TagId} not found", request.Id);
            throw new ResourceNotFoundException("Tag", request.Id.ToString());
        }

        var tagDto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };

        _logger.LogInformation("Successfully retrieved tag with ID: {TagId}", tag.Id);

        return tagDto;
    }
}