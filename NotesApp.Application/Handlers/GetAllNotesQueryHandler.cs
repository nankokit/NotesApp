using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetAllNotesQueryHandler : IRequestHandler<GetAllNotesQuery, PagedResult<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IMinioService _minioService;
    private readonly ILogger<GetAllNotesQueryHandler> _logger;

    public GetAllNotesQueryHandler(
        INoteRepository noteRepository,
        IMinioService minioService,
        ILogger<GetAllNotesQueryHandler> logger)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    try
                    {
                        var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
                        imageUrls.Add(url);
                        _logger.LogInformation("Generated presigned URL for image: {FileName}", fileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate presigned URL for image: {FileName}", fileName);
                        throw new ResourceNotFoundException("Image", fileName);
                    }
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

        var result = new PagedResult<NoteDto>
        {
            Items = noteDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation("Successfully retrieved {Count} notes", noteDtos.Count);

        return result;
    }
}