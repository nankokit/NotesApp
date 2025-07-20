using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotesApp.Application.Handlers;

public class GetAllNotesQueryHandler : IRequestHandler<GetAllNotesQuery, PagedResult<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IMinioService _minioService;

    public GetAllNotesQueryHandler(INoteRepository noteRepository, IMinioService minioService)
    {
        _noteRepository = noteRepository;
        _minioService = minioService;
    }

    public async Task<PagedResult<NoteDto>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _noteRepository.GetAllAsync(request.Search, request.Tags, request.SortBy, request.Ascending, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _noteRepository.CountAsync(request.Search, request.Tags, cancellationToken);

        var noteDtos = new List<NoteDto>();
        foreach (var note in notes)
        {
            var imageUrls = new List<string>();
            if (note.ImageFileNames != null)
            {
                foreach (var fileName in note.ImageFileNames)
                {
                    var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
                    imageUrls.Add(url);
                }
            }

            noteDtos.Add(new NoteDto
            {
                Id = note.Id,
                Name = note.Name,
                Description = note.Description,
                TagNames = note.Tags?.Select(tag => tag.Name).ToList(),
                ImageUrls = imageUrls,
                CreationDate = note.CreationDate
            });
        }

        return new PagedResult<NoteDto>
        {
            Items = noteDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}