using MediatR;
using System.IO;

namespace NotesApp.Application.Commands;

public class UploadImageCommand : IRequest<(string FileName, string Url)>
{
    public required Stream FileStream { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
}