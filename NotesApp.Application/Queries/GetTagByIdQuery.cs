using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Queries;

public class GetTagByIdQuery : IRequest<TagDto>
{
    public required Guid Id { get; set; }

}