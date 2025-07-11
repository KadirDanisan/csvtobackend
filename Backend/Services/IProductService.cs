// Services/ProductService.cs

using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Backend.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .AsNoTracking() // Sadece okuma yapacağımız için daha performanslıdır.
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<string> ProcessCsvUploadAsync(Stream fileStream)
        {
            // CsvHelper ile CSV'yi oku
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null };
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<CsvProductMap>();
            var records = csv.GetRecords<CsvProduct>().ToList();

            foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.SKU)) continue;

                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == record.SKU);
                if (existingProduct != null) continue; // Ürün varsa atla

                // Yeni Ürünü Oluştur
                var newProduct = new Product
                {
                    SKU = record.SKU,
                    Name = record.Name,
                    Price = record.Price,
                    Stock = record.Stock,
                    Description = record.Description,
                    ShortDescription = record.ShortDescription,
                    Images = record.Images
                };
                
                // Kategorileri İşle ve Ürüne Bağla
                if (!string.IsNullOrWhiteSpace(record.Categories))
                {
                    int? parentId = null;
                    var categoryNames = record.Categories.Split('>').Select(s => s.Trim());

                    foreach (var catName in categoryNames)
                    {
                        var existingCategory = await _context.Categories
                            .FirstOrDefaultAsync(c => c.Name == catName && c.ParentCategoryId == parentId);
                        
                        if (existingCategory == null)
                        {
                            existingCategory = new Category { Name = catName, ParentCategoryId = parentId };
                            _context.Categories.Add(existingCategory);
                            await _context.SaveChangesAsync();
                        }
                        parentId = existingCategory.Id;
                    }
                    
                    if (parentId.HasValue)
                    {
                        newProduct.ProductCategories = new List<ProductCategory>
                        {
                            new ProductCategory { CategoryId = parentId.Value }
                        };
                    }
                }
                
                _context.Products.Add(newProduct);
            }

            await _context.SaveChangesAsync();
            return $"{records.Count} satır okundu ve veritabanı başarıyla güncellendi.";
        }
    }

    // Bu yardımcı sınıflar burada veya ayrı bir dosyada durabilir.
    // Servis dosyası içinde kalması şimdilik sorun değil.
    public sealed class CsvProductMap : ClassMap<CsvProduct>
    {
        public CsvProductMap()
        {
            Map(m => m.SKU).Name("Stok kodu (SKU)");
            Map(m => m.Name).Name("İsim");
            Map(m => m.Price).Name("Normal fiyat").Default(0);
            Map(m => m.Stock).Name("Stok").Default(0);
            Map(m => m.Categories).Name("Kategoriler");
            Map(m => m.Description).Name("Açıklama");
            Map(m => m.ShortDescription).Name("Kısa açıklama");
            Map(m => m.Images).Name("Görseller");
        }
    }
    
    public class CsvProduct
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public string Categories { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string Images { get; set; }
    }
}