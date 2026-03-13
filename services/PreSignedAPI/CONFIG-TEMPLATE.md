# AWS S3 Presigned URL API - Configuration Template

This file shows how to configure the API with your AWS credentials.

## Local Development Setup

1. Copy this template and create `appsettings.Development.json`
2. Fill in your AWS credentials (do NOT commit to git)
3. The file is gitignored to prevent credential leaks

## Configuration Example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AWS": {
    "AccessKey": "AKIAIOSFODNN7EXAMPLE",
    "SecretKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
    "Region": "us-east-1",
    "BucketName": "my-video-bucket"
  },
  "PresignedUrl": {
    "ExpirationMinutes": 30,
    "UploadMaxSizeBytes": 104857600
  }
}
```

## Getting AWS Credentials

1. Go to [AWS IAM Console](https://console.aws.amazon.com/iam/)
2. Select your user or create one
3. Go to "Security Credentials" tab
4. Create/view Access Keys
5. Copy Access Key ID and Secret Access Key

## AWS Regions

Common regions:
- `us-east-1` - N. Virginia
- `us-west-2` - Oregon
- `eu-west-1` - Ireland
- `ap-southeast-1` - Singapore

See [AWS Regions](https://docs.aws.amazon.com/general/latest/gr/s3.html) for full list.

## Security Best Practices

⚠️ **NEVER commit credentials to version control**

For production, use:
- Environment variables
- AWS Secrets Manager
- AWS IAM Roles (if running on EC2/Lambda/ECS)
- Parameter Store

## Support

See [README.md](README.md) and [QUICKSTART.md](QUICKSTART.md) for more information.
