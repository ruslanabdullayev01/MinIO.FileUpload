using Microsoft.EntityFrameworkCore;
using Minio;
using MinIO.FileUpload.Services.Abstract;
using MinIO.FileUpload.Services.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("InMemoryDb"));

builder.Services.AddScoped<INewsService, NewsService>();

builder.Services.AddSingleton<MinioClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var minioConfig = configuration.GetSection("Minio");

    var endpoint = minioConfig["Endpoint"];
    var accessKey = minioConfig["AccessKey"];
    var secretKey = minioConfig["SecretKey"];
    var secure = bool.Parse(minioConfig["Secure"]);

    return (MinioClient)new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(secure)
        .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
