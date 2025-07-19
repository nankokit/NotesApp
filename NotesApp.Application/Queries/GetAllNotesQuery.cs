using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Queries;

public class GetAllNotesQuery : IRequest<PagedResult<NoteDto>>
{
    public string? Search { get; set; }
    public List<string>? Tags { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}