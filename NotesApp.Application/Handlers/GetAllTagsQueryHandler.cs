using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, PagedResult<TagDto>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetAllTagsQueryHandler> _logger;

    public GetAllTagsQueryHandler(ITagRepository tagRepository, ILogger<GetAllTagsQueryHandler> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<TagDto>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(request.Search, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _tagRepository.CountAsync(request.Search, cancellationToken);

        var tagDtos = tags.Select(tag => new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        }).ToList();

        var result = new PagedResult<TagDto>
        {
            Items = tagDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation("Successfully retrieved {Count} tags", tagDtos.Count);

        return result;
    }
}