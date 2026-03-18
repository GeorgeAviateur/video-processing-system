using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Xabe.FFmpeg;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace VideoProcessorWorker;

public class Worker(
    ILogger<Worker> logger,
    IAmazonSQS sqsClient,
    IAmazonS3 s3Client) : BackgroundService
{
    private readonly string _queueUrl = Environment.GetEnvironmentVariable("AWS_SQS_QUEUE_URL")!;
    private readonly string _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME")!;
    private readonly string _destinationFolder = "thumbnails/";
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Set FFmpeg path if needed
        FFmpeg.SetExecutablesPath("/usr/local/bin"); // Adjust path as needed

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 20
                }; 

                var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                if (response.Messages.Any())
                {
                    var message = response.Messages[0];
                    

                    
                    if (message.Body.Contains("s3:TestEvent"))
                    {
                        logger.LogInformation("Ignoring S3 test event");
                        await sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                        return;
                    }

                    var s3Event = JsonSerializer.Deserialize<S3Event>(message.Body);

                    var record = s3Event.Records[0];
                    var videoKey = Uri.UnescapeDataString(record.s3.@object.key);

                    logger.LogInformation("Processing video: {VideoKey}", videoKey);

                    await ProcessVideoAsync(videoKey, stoppingToken);

                    // Delete message after processing
                    await sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                }
                else
                {
                    logger.LogInformation("No messages in queue");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Queue URL: {QueueUrl}", _queueUrl);

                logger.LogError(ex, "Error processing message");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessVideoAsync(string videoKey, CancellationToken stoppingToken)
    {
        var tempVideoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(videoKey));
        var tempThumbDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(tempThumbDir);

            // Download video from S3
            var downloadRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = videoKey
            };

            using var response = await s3Client.GetObjectAsync(downloadRequest, stoppingToken);
            await using var videoStream = File.Create(tempVideoPath);
            await response.ResponseStream.CopyToAsync(videoStream, stoppingToken);

            // Create thumbnails
            var thumbnails = await CreateThumbnailsAsync(tempVideoPath, tempThumbDir, stoppingToken);

            // Upload thumbnails to S3
            foreach (var thumbPath in thumbnails)
            {
                var thumbKey = _destinationFolder + Path.GetFileNameWithoutExtension(videoKey) + "_thumb_" + Path.GetFileName(thumbPath);
                await UploadToS3Async(thumbPath, thumbKey, stoppingToken);
            }

            logger.LogInformation("Processed video {VideoKey}, created {Count} thumbnails", videoKey, thumbnails.Count);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempVideoPath))
                File.Delete(tempVideoPath);
            if (Directory.Exists(tempThumbDir))
                Directory.Delete(tempThumbDir, true);
        }
    }

    private async Task<List<string>> CreateThumbnailsAsync(string videoPath, string outputDir, CancellationToken stoppingToken)
    {
        var thumbnails = new List<string>();

        // Create thumbnails at 1s, 5s, 10s
        var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) };

        for (int i = 0; i < times.Length; i++)
        {
            var outputPath = Path.Combine(outputDir, $"thumb_{i:000}.jpg");
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, outputPath, times[i]);
            await conversion.Start(stoppingToken);
            thumbnails.Add(outputPath);
        }

        return thumbnails;
    }

    private async Task UploadToS3Async(string filePath, string key, CancellationToken stoppingToken)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            FilePath = filePath
        };

        await s3Client.PutObjectAsync(putRequest, stoppingToken);
    }
}
