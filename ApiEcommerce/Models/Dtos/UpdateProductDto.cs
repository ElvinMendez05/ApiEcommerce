namespace ApiEcommerce.Models.Dtos
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public int SKU { get; set; }
        public int Stok { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CategoryId { get; set; }
    }
}
