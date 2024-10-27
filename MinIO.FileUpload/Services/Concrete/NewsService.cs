using Microsoft.EntityFrameworkCore;
using MinIO.FileUpload.Models;
using MinIO.FileUpload.Services.Abstract;

namespace MinIO.FileUpload.Services.Concrete
{
    public class NewsService : INewsService
    {
        private readonly AppDbContext _context;

        public NewsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<News>> GetAllAsync()
        {
            return await _context.News.ToListAsync();
        }

        public async Task<News> GetByIdAsync(int id)
        {
            return await _context.News.FindAsync(id);
        }

        public async Task<News> CreateAsync(News news)
        {
            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<News> UpdateAsync(int id, News news)
        {
            var existingNews = await _context.News.FindAsync(id);
            if (existingNews != null)
            {
                existingNews.Title = news.Title;
                existingNews.ImagePath = news.ImagePath;
                await _context.SaveChangesAsync();
                return existingNews;
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem != null)
            {
                _context.News.Remove(newsItem);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}