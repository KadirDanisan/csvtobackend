// Services/IProductService.cs

using Backend.Models;

namespace Backend.Services
{
    // Projemizin iş mantığı operasyonlarını tanımlıyoruz.
    public interface IProductService
    {
        /// <summary>
        /// Sadece en üst seviyedeki (ana) kategorileri getirir.
        /// </summary>
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();

        /// <summary>
        /// Belirli bir kategori ID'sine göre ürünleri listeler.
        /// </summary>
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        /// <summary>
        /// ID'si verilen tek bir ürünün detaylarını kategorileriyle birlikte getirir.
        /// </summary>
        Task<Product?> GetProductByIdAsync(int productId);

        /// <summary>
        /// Gelen CSV dosyasının akışını (stream) işleyerek veritabanını günceller.
        /// </summary>
        /// <param name="fileStream">Yüklenen CSV dosyasının stream'i.</param>
        /// <returns>İşlem sonucunu özetleyen bir mesaj.</returns>
        Task<string> ProcessCsvUploadAsync(Stream fileStream);
    }
}