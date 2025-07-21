using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Infrastructure.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "notes-images";
    private readonly ILogger<MinioService> _logger;

    public MinioService(IMinioClient minioClient, ILogger<MinioService> logger)
    {
        _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
        bool bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken).ConfigureAwait(false);

        if (!bucketExists)
        {
            _logger.LogInformation("Bucket {BucketName} does not exist, creating it", _bucketName);
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Bucket {BucketName} created successfully", _bucketName);
        }

        var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Path.GetExtension(fileName)}";
        _logger.LogInformation("Generated unique file name: {UniqueFileName}", uniqueFileName);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(uniqueFileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        try
        {
            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Successfully uploaded image: {UniqueFileName}", uniqueFileName);
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", fileName);
            throw new FileOperationException("Upload", fileName, $"Failed to upload image: {fileName}", ex);
        }
    }

    public async Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        try
        {
            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;
            _logger.LogInformation("Successfully retrieved image: {FileName}", fileName);
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve image: {FileName}", fileName);
            throw new FileOperationException("GetImage", fileName, $"Failed to retrieve image: {fileName}", ex);
        }
    }

    public async Task<string> GetPresignedUrlAsync(string fileName, CancellationToken cancellationToken)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithExpiry(24 * 60 * 60);

        try
        {
            var url = await _minioClient.PresignedGetObjectAsync(args).ConfigureAwait(false);
            _logger.LogInformation("Successfully generated presigned URL for image: {FileName}", fileName);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate presigned URL for image: {FileName}", fileName);
            throw new FileOperationException("GetPresignedUrl", fileName, $"Failed to generate presigned URL for image: {fileName}", ex);
        }
    }

    public async Task DeleteImageAsync(string fileName, CancellationToken cancellationToken)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName);

        try
        {
            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Successfully deleted image: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {FileName}", fileName);
            throw new FileOperationException("Delete", fileName, $"Failed to delete image: {fileName}", ex);
        }
    }
}