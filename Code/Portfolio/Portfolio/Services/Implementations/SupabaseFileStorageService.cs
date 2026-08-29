using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Portfolio.Services.Interfaces;

namespace PortfolioApi.Services.Implementations;

// Supabase Storage exposes an S3-compatible API, so we reuse the AWS S3 SDK
// pointed at Supabase's S3 endpoint instead of AWS's. Implements the exact
// same IFileStorageService contract as R2FileStorageService did — this is
// what lets us swap storage providers without touching any controller/service
// that depends on IFileStorageService.
public class SupabaseFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    public SupabaseFileStorageService(IConfiguration config)
    {
        var accessKey = config["Storage:AccessKey"];
        var secretKey = config["Storage:SecretKey"];
        var endpoint = config["Storage:Endpoint"]
            ?? throw new InvalidOperationException("Storage:Endpoint not configured");
        _bucketName = config["Storage:BucketName"]
            ?? throw new InvalidOperationException("Storage:BucketName not configured");
        _publicBaseUrl = (config["Storage:PublicBaseUrl"]
            ?? throw new InvalidOperationException("Storage:PublicBaseUrl not configured")).TrimEnd('/');

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true, // required for S3-compatible providers like Supabase/R2
            AuthenticationRegion = config["Storage:Region"] ?? "us-east-1",
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder, string[] allowedContentTypes, long maxSizeBytes)
    {
        if (file.Length == 0) throw new ArgumentException("Empty file.");
        if (file.Length > maxSizeBytes) throw new ArgumentException($"File exceeds the {maxSizeBytes / (1024 * 1024)}MB limit.");
        if (!allowedContentTypes.Contains(file.ContentType)) throw new ArgumentException("File type not allowed.");

        var extension = Path.GetExtension(file.FileName);
        var key = $"{folder}/{Guid.NewGuid()}{extension}";

        using var stream = file.OpenReadStream();
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
        });

        return key; // store just the key — public URL is resolved separately via GetPublicUrl
    }

    public async Task DeleteFileAsync(string? storedUrl)
    {
        if (string.IsNullOrWhiteSpace(storedUrl)) return;

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storedUrl,
        });
    }

    public async Task<(Stream Stream, string ContentType)> GetFileStreamAsync(string storedUrl)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, storedUrl);
        return (response.ResponseStream, response.Headers.ContentType);
    }

    public string? GetPublicUrl(string? storedKey)
    {
        if (string.IsNullOrWhiteSpace(storedKey)) return null;
        return $"{_publicBaseUrl}/{storedKey}";
    }
}