namespace ComputerStore.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = "Default description"; // Default description value

        public ICollection<Product> Products { get; set; }

        public Category()
        {
            Products = new HashSet<Product>();
        }

        public List<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}