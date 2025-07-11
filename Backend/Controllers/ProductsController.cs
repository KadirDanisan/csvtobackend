// Controllers/ProductsController.cs

using Microsoft.AspNetCore.Mvc;
using Backend.Services; // Artık sadece servisi kullanacağız

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/products")] // Route'u sadeleştirdim
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        // Controller artık DbContext'i değil, IProductService'i istiyor.
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("categories/toplevel")]
        public async Task<IActionResult> GetTopLevelCategories()
        {
            var categories = await _productService.GetTopLevelCategoriesAsync();
            return Ok(categories);
        }
        
        [HttpGet("categories/{categoryId}/products")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) return NotFound();
            
            return Ok(product);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Lütfen bir dosya seçin.");
            
            try
            {
                // Dosya akışını (stream) doğrudan servise gönderiyoruz.
                var resultMessage = await _productService.ProcessCsvUploadAsync(file.OpenReadStream());
                return Ok(new { message = resultMessage });
            }
            catch (Exception ex)
            {
                // Hata oluşursa yakalayıp 500 koduyla dönüyoruz.
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }
    }
}