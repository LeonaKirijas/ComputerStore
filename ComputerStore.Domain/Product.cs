using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name must be less than 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number")]
        public int Quantity { get; set; }

        // Many-to-many relationship via ProductCategory
        public ICollection<ProductCategory> ProductCategories { get; set; } = new HashSet<ProductCategory>();

        public Product()
        {
            ProductCategories = new HashSet<ProductCategory>();
        }
    }
}
