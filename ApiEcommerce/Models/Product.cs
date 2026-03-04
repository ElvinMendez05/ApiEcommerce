using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEcommerce.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string ImgUrl { get; set; } = string.Empty;

        [Required]
        public int SKU { get; set; } //PROD-OO1-BLK-M
        [Range(0, int.MaxValue)]
        public int Stok { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        //Relationship between Category and Product 
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public required Category Category { get; set; }
    }
}
