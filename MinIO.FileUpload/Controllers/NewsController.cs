using Microsoft.AspNetCore.Mvc;
using Minio;
using MinIO.FileUpload.DTOs;
using MinIO.FileUpload.Models;
using MinIO.FileUpload.Services.Abstract;

namespace MinIO.FileUpload.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly MinioClient _minioClient;
        private readonly string BacketName = "news";

        public NewsController(INewsService newsService, MinioClient minioClient)
        {
            _newsService = newsService;
            _minioClient = minioClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var newsList = await _newsService.GetAllAsync();
            return Ok(newsList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var newsItem = await _newsService.GetByIdAsync(id);
            if (newsItem == null)
                return NotFound();

            return Ok(newsItem);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] NewsDto newsDto)
        {
            if (newsDto == null || newsDto.File == null || newsDto.File.Length == 0)
                return BadRequest("Invalid news data or file.");

            var stream = newsDto.File.OpenReadStream();
            var fileSize = newsDto.File.Length;
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newsDto.File.FileName);

            await _minioClient.UploadFileAsync(BacketName, fileName, stream, fileSize);

            var news = new News
            {
                Title = newsDto.Title,
                ImagePath = $"{fileName}"
            };

            var createdNews = await _newsService.CreateAsync(news);
            return CreatedAtAction(nameof(GetById), new { id = createdNews.Id }, createdNews);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] NewsDto newsDto)
        {
            if (newsDto == null)
                return BadRequest("Invalid news data.");

            var news = new News
            {
                Title = newsDto.Title
            };

            if (newsDto.File != null && newsDto.File.Length > 0)
            {
                var stream = newsDto.File.OpenReadStream();
                var fileSize = newsDto.File.Length;
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newsDto.File.FileName);

                await _minioClient.UploadFileAsync(BacketName, fileName, stream, fileSize);
                news.ImagePath = fileName;
            }

            var updatedNews = await _newsService.UpdateAsync(id, news);
            if (updatedNews == null)
                return NotFound();

            return Ok(updatedNews);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _newsService.DeleteAsync(id);
            if (!deleted)  return NotFound();
            return NoContent();
        }
    }
}
