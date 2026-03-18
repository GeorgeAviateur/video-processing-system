# Video Processor Worker

A .NET Worker Service that processes videos from AWS S3 via SQS messages.

## Features

- Consumes messages from AWS SQS queue
- Downloads videos from AWS S3 bucket
- Generates thumbnails using FFmpeg
- Uploads thumbnails back to S3
- Deletes processed messages from queue

## Setup

1. Install FFmpeg:
   ```bash
   brew install ffmpeg
   ```

2. Configure AWS credentials (via AWS CLI, environment variables, or IAM roles)

3. Update `appsettings.json` with your SQS queue URL and S3 bucket name:
   ```json
   {
     "AWS": {
       "SQSQueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789012/my-queue",
       "S3BucketName": "my-video-bucket",
       "Region": "us-east-1"
     }
   }
   ```

4. Run the worker:
   ```bash
   dotnet run
   ```

## Message Format

SQS message body should contain the S3 key of the video file, e.g.:
```
videos/my-video.mp4
```

## Thumbnails

Creates thumbnails at 1s, 5s, and 10s intervals, uploaded as:
- `my-video_thumb_000.jpg`
- `my-video_thumb_001.jpg`
- `my-video_thumb_002.jpg`