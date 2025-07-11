// Models/Product.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        // SKU hala önemli
        [Required]
        public string SKU { get; set; }

        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? Images { get; set; }
        
        // Bu ürünü kategorilere bağlamak için
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
    }
}