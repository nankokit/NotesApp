using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Queries;

public class GetAllTagsQuery : IRequest<PagedResult<TagDto>>
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}