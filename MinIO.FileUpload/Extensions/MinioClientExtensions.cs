using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;

public static class MinioClientExtensions
{
    public static async Task UploadFileAsync(this MinioClient minioClient, string bucketName, string fileName, Stream fileStream, long fileSize)
    {
        try
        {
            bool bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!bucketExists)
            {
                await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

            await minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileSize));
        }
        catch (MinioException ex)
        {
            throw new Exception($"Error loading file: {ex.Message}");
        }
    }

    public static async Task<Stream> DownloadFileAsync(this MinioClient minioClient, string bucketName, string fileName)
    {
        var memoryStream = new MemoryStream();
        try
        {
            await minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithCallbackStream(async stream =>
                {
                    await stream.CopyToAsync(memoryStream);
                }));

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (MinioException ex)
        {
            throw new Exception($"Error downloading file: {ex.Message}");
        }
    }

    [Obsolete]
    public static async Task<List<string>> GetAllFilesAsync(this MinioClient minioClient, string bucketName)
    {
        var objectList = new List<string>();

        try
        {
            bool bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!bucketExists)
                throw new Exception($"Bucket '{bucketName}' does not exist.");

            var objectArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true);

            var tcs = new TaskCompletionSource<bool>();

            minioClient.ListObjectsAsync(objectArgs).Subscribe(
                item =>
                {
                    objectList.Add(item.Key);
                },
                ex =>
                {
                    tcs.SetException(ex);
                },
                () =>
                {
                    tcs.SetResult(true);
                }
            );

            await tcs.Task;

            return objectList;
        }
        catch (MinioException ex)
        {
            throw new Exception($"Error retrieving files: {ex.Message}");
        }
    }
}
