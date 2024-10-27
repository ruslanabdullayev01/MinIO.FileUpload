using MinIO.FileUpload.Models;

namespace MinIO.FileUpload.Services.Abstract
{
    public interface INewsService
    {
        Task<IEnumerable<News>> GetAllAsync();
        Task<News> GetByIdAsync(int id);
        Task<News> CreateAsync(News news);
        Task<News> UpdateAsync(int id, News news);
        Task<bool> DeleteAsync(int id);
    }

}
