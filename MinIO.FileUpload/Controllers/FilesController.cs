using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MinIO.FileUpload.DTOs;

namespace MinIO.FileUpload.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly MinioClient _minioClient;
        private const string BucketName = "files";

        public FilesController(MinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto model)
        {
            if (model.File == null || model.File.Length == 0) return BadRequest("The file was not uploaded.");

            var fileName = model.File.FileName;
            using var stream = model.File.OpenReadStream();

            try
            {
                await _minioClient.UploadFileAsync(BucketName, fileName, stream, model.File.Length);
                return Ok(new { message = "File uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var fileStream = await _minioClient.DownloadFileAsync(BucketName, fileName);
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (MinioException ex)
            {
                return StatusCode(500, $"File download error: {ex.Message}");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllFiles()
        {
            try
            {
                var files = await _minioClient.GetAllFilesAsync(BucketName);
                return Ok(files);
            }
            catch (MinioException ex)
            {
                return StatusCode(500, $"Error retrieving files: {ex.Message}");
            }
        }
    }
}
