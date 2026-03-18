using VideoProcessorWorker;
using Amazon.SQS;
using Amazon.S3;

var builder = Host.CreateApplicationBuilder(args);

// Configure AWS credentials from environment variables
var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
var awsSecretKey =  Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");

if (string.IsNullOrEmpty(awsAccessKey) || string.IsNullOrEmpty(awsSecretKey))
{
    throw new InvalidOperationException("AWS credentials not found. Please set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY environment variables.");
}

// Create AWS clients with explicit credentials
var sqsClient = new AmazonSQSClient(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.GetBySystemName(awsRegion));
var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.GetBySystemName(awsRegion));

// Register services
builder.Services.AddSingleton<IAmazonSQS>(sqsClient);
builder.Services.AddSingleton<IAmazonS3>(s3Client);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
