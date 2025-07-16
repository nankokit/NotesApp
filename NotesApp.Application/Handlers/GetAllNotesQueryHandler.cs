using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetAllNotesQueryHandler : IRequestHandler<GetAllNotesQuery, PagedResult<NoteDto>>
{
    private readonly INoteRepository _noteRepository;

    public GetAllNotesQueryHandler(INoteRepository noteRepository) => _noteRepository = noteRepository;

    public async Task<PagedResult<NoteDto>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _noteRepository.GetAllAsync(request.Search, request.Tags, request.SortBy, request.Ascending, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _noteRepository.CountAsync(request.Search, request.Tags, cancellationToken);

        return new PagedResult<NoteDto>
        {
            Items = notes.Select(note => new NoteDto
            {
                Id = note.Id,
                Name = note.Name,
                Description = note.Description,
                TagNames = note.Tags.Select(tag => tag.Name).ToList(),
                ImageUrls = note.ImageUrls,
                CreationDate = note.CreationDate
            }).ToList(),

            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}