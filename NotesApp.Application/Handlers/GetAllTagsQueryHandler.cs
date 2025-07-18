// File: NotesApp.Application/Handlers/GetAllTagsQueryHandler.cs
using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, PagedResult<TagDto>>
{
    private readonly ITagRepository _tagRepository;

    public GetAllTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<PagedResult<TagDto>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(request.Search, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _tagRepository.CountAsync(request.Search, cancellationToken);

        return new PagedResult<TagDto>
        {
            Items = tags.Select(tag => new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            }).ToList(),
            TotalCount = totalCount
        };
    }
}