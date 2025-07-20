using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly IMinioService _minioService;

    public GetNoteByIdQueryHandler(INoteRepository noteRepository, IMinioService minioService)
    {
        _noteRepository = noteRepository;
        _minioService = minioService;
    }

    public async Task<NoteDto> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
        if (note == null) throw new Exception("Note not found");

        var imageUrls = new List<string>();
        if (note.ImageFileNames != null)
        {
            foreach (var fileName in note.ImageFileNames)
            {
                var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
                imageUrls.Add(url);
            }
        }

        return new NoteDto
        {
            Id = note.Id,
            Name = note.Name,
            Description = note.Description,
            TagNames = note.Tags?.Select(tag => tag.Name).ToList(),
            ImageUrls = imageUrls,
            CreationDate = note.CreationDate
        };
    }

}