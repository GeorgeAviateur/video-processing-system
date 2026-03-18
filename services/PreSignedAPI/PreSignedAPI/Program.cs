using Amazon.S3;
using PreSignedAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configure AWS S3 Client
var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");

var s3Client = new AmazonS3Client(
    awsAccessKey,
    awsSecretKey,
    Amazon.RegionEndpoint.GetBySystemName(awsRegion)
);

builder.Services.AddSingleton<IAmazonS3>(s3Client);
builder.Services.AddScoped<IS3PresignedUrlService, S3PresignedUrlService>();

// Add CORS for frontend access (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();

