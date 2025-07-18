using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetTagByNameQueryHandler : IRequestHandler<GetTagByNameQuery, TagDto>
{
    private readonly ITagRepository _tagRepository;

    public GetTagByNameQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<TagDto> Handle(GetTagByNameQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with name '{request.Name}' not found");
        }
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}