using Amazon.S3;
using Amazon.S3.Model;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Implementations
{
    public class R2FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _publicBaseUrl;

        public R2FileStorageService(IConfiguration config)
        {
            var accessKey = config["R2:AccessKey"];
            var secretKey = config["R2:SecretKey"];
            var accountId = config["R2:AccountId"];
            _bucketName = config["R2:BucketName"] ?? throw new InvalidOperationException("R2:BucketName not configured");
            _publicBaseUrl = (config["R2:PublicBaseUrl"] ?? throw new InvalidOperationException("R2:PublicBaseUrl not configured"))
                .TrimEnd('/');

            var s3Config = new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
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

            return key; // still store just the key in the database — see GetPublicUrl below
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
}
