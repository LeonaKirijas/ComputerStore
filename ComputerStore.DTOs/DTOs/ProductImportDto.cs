using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.DTOs
{
    /// <summary>
    /// Represents a product data transfer object for importing product data.
    /// </summary>
    public class ProductImportDto
    {
        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the categories associated with the product.
        /// </summary>
        public List<string> Categories { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the product in stock.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the description of the product.
        /// </summary>
        public string Description { get; set; } // Add this line
    }
}