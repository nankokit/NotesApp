using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NotesApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImageController : ControllerBase
{
    private readonly IMinioService _minioService;
    private readonly MediatR.IMediator _mediator;

    public ImageController(IMinioService minioService, MediatR.IMediator mediator)
    {
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var command = new UploadImageCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            var (uniqueFileName, presignedUrl) = await _mediator.Send(command, cancellationToken);

            return Ok(new { FileName = uniqueFileName, Url = presignedUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading image: {ex.Message}");
        }
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetImage(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var stream = await _minioService.GetImageAsync(fileName, cancellationToken);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            return NotFound($"Image not found: {ex.Message}");
        }
    }

    [HttpGet("url/{fileName}")]
    public async Task<IActionResult> GetImageUrl(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
            return Ok(new { Url = url });
        }
        catch (Exception ex)
        {
            return NotFound($"Image not found: {ex.Message}");
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteImage(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            await _minioService.DeleteImageAsync(fileName, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound($"Image not found: {ex.Message}");
        }
    }
}