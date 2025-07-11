using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Category
    {
           [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Alt kategorileri bağlamak için kullanılır
        public int? ParentCategoryId { get; set; }
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }

        // Bu kategorideki ürünleri bağlamak için
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
   
    }
}