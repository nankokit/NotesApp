using Minio;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Infrastructure.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "notes-images";

    public MinioService(IMinioClient minioClient)
    {
        _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
        bool bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken).ConfigureAwait(false);

        if (!bucketExists)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken).ConfigureAwait(false);
        }

        var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Path.GetExtension(fileName)}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(uniqueFileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

        return uniqueFileName;
    }

    public async Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken).ConfigureAwait(false);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<string> GetPresignedUrlAsync(string fileName, CancellationToken cancellationToken)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithExpiry(24 * 60 * 60);

        return await _minioClient.PresignedGetObjectAsync(args).ConfigureAwait(false);
    }

    public async Task DeleteImageAsync(string fileName, CancellationToken cancellationToken)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName);

        await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken).ConfigureAwait(false);
    }
}