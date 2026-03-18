using Amazon.S3;
using Amazon.S3.Model;

namespace PreSignedAPI.Services;

public interface IS3PresignedUrlService
{
    Task<string> GenerateDownloadUrlAsync(string objectKey);
    Task<string> GenerateUploadUrlAsync(string objectKey);
    Task RevokePresignedUrlAsync(string objectKey);
    Task<List<string>> ListRequestsAsync(string prefix = "");
}

public class S3PresignedUrlService : IS3PresignedUrlService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3PresignedUrlService> _logger;

    public S3PresignedUrlService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3PresignedUrlService> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateDownloadUrlAsync(string objectKey)
    {
        try
        {
            
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
            var expirationMinutes = int.Parse(_configuration["PresignedUrl:ExpirationMinutes"] ?? "30");

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Verb = HttpVerb.GET
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Generated download URL for {ObjectKey}", objectKey);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating download URL: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GenerateUploadUrlAsync(string objectKey)
    {
        try
        {
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
            var expirationMinutes = int.Parse(_configuration["PresignedUrl:ExpirationMinutes"] ?? "30");

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Verb = HttpVerb.PUT
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation($"Generated upload URL for {objectKey}");
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating upload URL: {ex.Message}");
            throw;
        }
    }

    public async Task RevokePresignedUrlAsync(string objectKey)
    {
        try
        {
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");

            // Note: Presigned URLs cannot be revoked directly. 
            // This method demonstrates best practices:
            // 1. Delete the object from S3 (invalidates download URLs)
            // 2. Or rotate AWS credentials
            // 3. Or use a permission-based approach with custom headers

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
            _logger.LogInformation($"Revoked access by deleting object: {objectKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error revoking URL: {ex.Message}");
            throw;
        }
    }

    public async Task<List<string>> ListRequestsAsync(string prefix = "")
    {
        try
        {
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(listRequest);
            var keys = response.S3Objects.Select(obj => obj.Key).ToList();
            _logger.LogInformation($"Listed {keys.Count} objects with prefix: {prefix}");
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error listing objects: {ex.Message}");
            throw;
        }
    }
}
