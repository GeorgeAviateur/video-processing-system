# Quick Start Guide - Presigned API

## 5-Minute Setup

### Step 1: Get Your AWS Credentials
1. Go to [AWS Console](https://console.aws.amazon.com)
2. Navigate to IAM → Users → Your User → Security Credentials
3. Create an Access Key (or use existing one)
4. Copy the Access Key ID and Secret Access Key

### Step 2: Create an S3 Bucket
1. Navigate to S3 in AWS Console
2. Click "Create Bucket"
3. Choose a unique name (e.g., `video-thumbnails-api`)
4. Click "Create"

### Step 3: Update Configuration
Edit `appsettings.Development.json`:

```json
{
  "AWS": {
    "AccessKey": "AKIAIOSFODNN7EXAMPLE",
    "SecretKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
    "Region": "us-east-1",
    "BucketName": "video-thumbnails-api"
  },
  "PresignedUrl": {
    "ExpirationMinutes": 30,
    "UploadMaxSizeBytes": 104857600
  }
}
```

### Step 4: Run the API
```bash
dotnet run
```

API available at: `https://localhost:5001`

### Step 5: Test an Endpoint
```bash
curl "https://localhost:5001/api/presignedurl/health"
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2026-03-12T12:30:45Z"
}
```

## Common Tasks

### Upload a File to S3

```bash
# Step 1: Get upload URL
UPLOAD_URL=$(curl -s "https://localhost:5001/api/presignedurl/upload?objectKey=videos/my-video.mp4" | jq -r '.url')

# Step 2: Upload file using the presigned URL
curl -X PUT --data-binary @/path/to/video.mp4 "$UPLOAD_URL"
```

### Download a File

```bash
# Get download URL
DOWNLOAD_URL=$(curl -s "https://localhost:5001/api/presignedurl/download?objectKey=videos/my-video.mp4" | jq -r '.url')

# Download file
curl "$DOWNLOAD_URL" -o downloaded-video.mp4
```

### List Files

```bash
curl "https://localhost:5001/api/presignedurl/list?prefix=videos/"
```

## Connecting from Frontend

### React/JavaScript Example

```javascript
// Get presigned upload URL from API
const getUploadUrl = async (fileName) => {
  const response = await fetch(
    `https://localhost:5001/api/presignedurl/upload?objectKey=${fileName}`
  );
  return await response.json();
};

// Handle file upload
const handleFileUpload = async (file) => {
  const data = await getUploadUrl(`uploads/${Date.now()}-${file.name}`);
  
  const uploadResponse = await fetch(data.url, {
    method: 'PUT',
    body: file,
    headers: { 'Content-Type': file.type }
  });
  
  if (uploadResponse.ok) {
    console.log('Upload successful!');
  }
};
```

## Environment Variables (Production)

Instead of config files, use environment variables:

```bash
export AWS_ACCESS_KEY_ID=your_key
export AWS_SECRET_ACCESS_KEY=your_secret
export AWS_REGION=us-east-1
export BUCKET_NAME=your-bucket
```

Update Program.cs to read these:
```csharp
var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
```

## Troubleshooting

### 403 Forbidden
- ❌ AWS credentials are incorrect or lack S3 permissions
- ✅ Check IAM policy includes S3 full access

### 404 NoSuchBucket
- ❌ Bucket name doesn't exist or is in different region
- ✅ Verify bucket name matches exactly (case-sensitive)

### Connection Refused
- ❌ API is not running
- ✅ Run `dotnet run` first

### HTTPS Certificate Warning
- ❌ This is expected for self-signed dev certificate
- ✅ Add `-k` to curl to skip verification: `curl -k https://...`

## Next: Production Deployment

See [README.md](./README.md) for:
- Docker deployment
- AWS Lambda integration
- Error handling examples
- API documentation
