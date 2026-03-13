using Microsoft.AspNetCore.Mvc;
using PreSignedAPI.Services;
using Amazon.S3;



namespace PreSignedAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PresignedUrlController : ControllerBase
{
    private readonly IS3PresignedUrlService _s3Service;
    private readonly ILogger<PresignedUrlController> _logger;

    public PresignedUrlController(IS3PresignedUrlService s3Service, ILogger<PresignedUrlController> logger)
    {
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Generate a presigned URL for downloading a file from S3
    /// </summary>
    /// <param name="objectKey">The S3 object key (path) of the file</param>
    /// <returns>Presigned URL for download</returns>
    [HttpGet("download")]
    [ProducesResponseType(typeof(PresignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateDownloadUrl([FromQuery] string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return BadRequest(new { error = "objectKey is required" });
        }

        try
        {
            var url = await _s3Service.GenerateDownloadUrlAsync(objectKey);
            return Ok(new PresignedUrlResponse
            {
                Url = url,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse("30")),
                ObjectKey = objectKey,
                Operation = "download"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GenerateDownloadUrl: {ex.Message}");
            return StatusCode(500, new { error = "Failed to generate download URL", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate a presigned URL for uploading a file to S3
    /// </summary>
    /// <param name="objectKey">The S3 object key (path) where the file will be uploaded</param>
    /// <returns>Presigned URL for upload</returns>
    [HttpGet("upload")]
    [ProducesResponseType(typeof(PresignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateUploadUrl([FromQuery] string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return BadRequest(new { error = "objectKey is required" });
        }

        try
        {
            var url = await _s3Service.GenerateUploadUrlAsync(objectKey);
            return Ok(new PresignedUrlResponse
            {
                Url = url,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse("30")),
                ObjectKey = objectKey,
                Operation = "upload"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GenerateUploadUrl: {ex.Message}");
            return StatusCode(500, new { error = "Failed to generate upload URL", details = ex.Message });
        }
    }

    /// <summary>
    /// Revoke access to an object by deleting it from S3
    /// </summary>
    /// <param name="objectKey">The S3 object key to revoke access to</param>
    /// <returns>Status of the revocation</returns>
    [HttpDelete("revoke")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeAccess([FromQuery] string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return BadRequest(new { error = "objectKey is required" });
        }

        try
        {
            await _s3Service.RevokePresignedUrlAsync(objectKey);
            return Ok(new { message = "Access revoked successfully", objectKey });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in RevokeAccess: {ex.Message}");
            return StatusCode(500, new { error = "Failed to revoke access", details = ex.Message });
        }
    }

    /// <summary>
    /// List all objects in the S3 bucket (optionally filtered by prefix)
    /// </summary>
    /// <param name="prefix">Optional prefix to filter objects</param>
    /// <returns>List of object keys</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ListObjectsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListObjects([FromQuery] string prefix = "")
    {
        try
        {
            var objects = await _s3Service.ListRequestsAsync(prefix);
            return Ok(new ListObjectsResponse
            {
                ObjectKeys = objects,
                Count = objects.Count,
                Prefix = prefix
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ListObjects: {ex.Message}");
            return StatusCode(500, new { error = "Failed to list objects", details = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

public class PresignedUrlResponse
{
    public string Url { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string ObjectKey { get; set; }
    public string Operation { get; set; }
}

public class ListObjectsResponse
{
    public List<string> ObjectKeys { get; set; }
    public int Count { get; set; }
    public string Prefix { get; set; }
}
