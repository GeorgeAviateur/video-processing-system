# Presigned API - AWS S3 Integration

A .NET 10 Web API for generating and managing AWS S3 presigned URLs. This API allows secure, temporary access to S3 objects without exposing AWS credentials.

## Features

- ✅ Generate presigned URLs for downloading files from S3
- ✅ Generate presigned URLs for uploading files to S3
- ✅ Revoke access by deleting objects
- ✅ List objects in S3 bucket with optional prefix filtering
- ✅ Built-in health check endpoint
- ✅ Comprehensive error handling and logging

## Prerequisites

- .NET 10 SDK
- AWS Account with S3 bucket
- AWS Access Key and Secret Key

## Setup Instructions

### 1. Install Dependencies

```bash
dotnet restore
```

The AWSSDK.S3 NuGet package is already added to the project file.

### 2. Configure AWS Credentials

Edit `appsettings.json` and replace the placeholders:

```json
{
  "AWS": {
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY",
    "Region": "us-east-1",
    "BucketName": "your-bucket-name"
  },
  "PresignedUrl": {
    "ExpirationMinutes": 30,
    "UploadMaxSizeBytes": 104857600
  }
}
```

**Important:** For production, use AWS Secrets Manager or environment variables instead of storing credentials in config files.

### 3. Build & Run

```bash
dotnet build
dotnet run
```

The API will be available at `https://localhost:5001` (development) or the configured URL.

## API Endpoints

### 1. Generate Download Presigned URL

```http
GET /api/presignedurl/download?objectKey=path/to/file.mp4
```

**Response:**
```json
{
  "url": "https://your-bucket.s3.amazonaws.com/path/to/file.mp4?...",
  "expiresAt": "2026-03-12T15:30:00Z",
  "objectKey": "path/to/file.mp4",
  "operation": "download"
}
```

### 2. Generate Upload Presigned URL

```http
GET /api/presignedurl/upload?objectKey=path/to/new-file.mp4
```

**Response:**
```json
{
  "url": "https://your-bucket.s3.amazonaws.com/path/to/new-file.mp4?...",
  "expiresAt": "2026-03-12T15:30:00Z",
  "objectKey": "path/to/new-file.mp4",
  "operation": "upload"
}
```

### 3. Revoke Access (Delete Object)

```http
DELETE /api/presignedurl/revoke?objectKey=path/to/file.mp4
```

**Response:**
```json
{
  "message": "Access revoked successfully",
  "objectKey": "path/to/file.mp4"
}
```

### 4. List Objects

```http
GET /api/presignedurl/list?prefix=thumbnails/
```

**Response:**
```json
{
  "objectKeys": [
    "thumbnails/video1.jpg",
    "thumbnails/video2.jpg"
  ],
  "count": 2,
  "prefix": "thumbnails/"
}
```

### 5. Health Check

```http
GET /api/presignedurl/health
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-03-12T12:30:45Z"
}
```

## Usage Examples

### Using cURL

**Generate download URL:**
```bash
curl "https://localhost:5001/api/presignedurl/download?objectKey=videos/sample.mp4"
```

**Generate upload URL:**
```bash
curl "https://localhost:5001/api/presignedurl/upload?objectKey=videos/upload.mp4"
```

**List objects:**
```bash
curl "https://localhost:5001/api/presignedurl/list?prefix=videos/"
```

### Using JavaScript/Fetch

```javascript
// Get download URL
const response = await fetch(
  'https://localhost:5001/api/presignedurl/download?objectKey=videos/sample.mp4'
);
const data = await response.json();
const downloadUrl = data.url;

// Download the file using the presigned URL
const file = await fetch(downloadUrl);
const blob = await file.blob();
```

```javascript
// Get upload URL and upload a file
const response = await fetch(
  'https://localhost:5001/api/presignedurl/upload?objectKey=videos/upload.mp4'
);
const data = await response.json();
const uploadUrl = data.url;

// Upload a file using the presigned URL
const file = new File(['...'], 'upload.mp4');
await fetch(uploadUrl, {
  method: 'PUT',
  body: file,
  headers: { 'Content-Type': 'video/mp4' }
});
```

## Security Considerations

1. **Credentials Management:**
   - Never commit credentials to version control
   - Use environment variables or AWS Secrets Manager in production
   - Rotate access keys regularly

2. **URL Expiration:**
   - Presigned URLs expire after the configured time (default: 30 minutes)
   - Adjust `ExpirationMinutes` in appsettings based on your needs

3. **CORS:**
   - The API enables CORS for all origins in development
   - Configure appropriately for production

4. **HTTPS:**
   - Always use HTTPS in production
   - Presigned URLs should only be transmitted over encrypted connections

5. **Access Control:**
   - Consider adding authentication/authorization middleware
   - Implement rate limiting to prevent abuse

## Production Deployment

1. **Use AWS IAM Roles:**
   ```csharp
   var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);
   // Uses IAM role credentials automatically
   ```

2. **Environment Variables:**
   ```bash
   export AWS_ACCESS_KEY_ID=your_key
   export AWS_SECRET_ACCESS_KEY=your_secret
   export AWS_REGION=us-east-1
   ```

3. **Docker Example:**
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
   WORKDIR /src
   COPY . .
   RUN dotnet publish -c Release -o /app

   FROM mcr.microsoft.com/dotnet/aspnet:10.0
   WORKDIR /app
   COPY --from=build /app .
   EXPOSE 5000
   ENV ASPNETCORE_URLS=http://+:5000
   ENTRYPOINT ["dotnet", "PreSignedAPI.dll"]
   ```

## Project Structure

```
PreSignedAPI/
├── Controllers/
│   └── PresignedUrlController.cs    # API endpoints
├── Services/
│   └── S3PresignedUrlService.cs     # S3 business logic
├── Program.cs                       # Application setup
├── appsettings.json                 # Configuration
└── PreSignedAPI.csproj              # Project file
```

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK` - Successful request
- `400 Bad Request` - Invalid parameters
- `500 Internal Server Error` - Server-side error with details

Example error response:
```json
{
  "error": "Failed to generate download URL",
  "details": "The specified bucket does not exist"
}
```

## Troubleshooting

### "The specified bucket does not exist"
- Verify bucket name in appsettings.json
- Ensure AWS credentials have S3 permissions

### "Access Denied"
- Check AWS Access Key and Secret Key
- Verify IAM policy grants S3 permissions
- Check S3 bucket policy

### Connection Timeout
- Verify AWS region is correct
- Check network connectivity
- Verify SSL certificates in development

## Next Steps

- Add authentication/authorization middleware
- Implement database for audit logging
- Add retry logic for failed operations
- Create metrics/monitoring
- Add batch operations support

## License

MIT
