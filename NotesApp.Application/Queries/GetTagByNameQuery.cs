using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Queries;

public class GetTagByNameQuery : IRequest<TagDto>
{
    public required string Name { get; set; }

}