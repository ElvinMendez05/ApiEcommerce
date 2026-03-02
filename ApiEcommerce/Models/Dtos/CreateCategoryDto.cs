using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Name is mandatory")]
        [MaxLength(50, ErrorMessage = "Name cannot have more than 50 caracters")]
        [MinLength(3, ErrorMessage = "Name cannot have less than 3 caracters")]
        public string Name { get; set; } = string.Empty;
    }
}
