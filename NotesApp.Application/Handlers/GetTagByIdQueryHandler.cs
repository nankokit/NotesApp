using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, TagDto>
{
    private readonly ITagRepository _tagRepository;

    public GetTagByIdQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<TagDto> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with ID '{request.Id}' not found");
        }
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}