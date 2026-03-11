using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, IMapper mapper, 
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"The product with the id {productId} it doesn't exist");
            }

            var productDto = _mapper.Map<ProductDto>(product);

            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("Customer", "The product is already exist");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("Customer", $"The category with the id {createProductDto.CategoryId} is already exist");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("Customer", $"Something went wrong saving register {product.Name}");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createdProduct);

            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }

        [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductsByCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var product = _productRepository.GetProductsForCategory(categoryId);

            if (product.Count == 0)
            {
                return NotFound($"The productS with the category {categoryId} it doesn't exist");
            }
            
            var productDto = _mapper.Map<List<ProductDto>>(product);
            return Ok(productDto);
        }

        [HttpGet("searchProductByName/{searchTerm}", Name = "GetProductsByName")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsByName(string searchTerm)
        {
            var product = _productRepository.SearchProducts(searchTerm);

            if (product.Count == 0)
            {
                return NotFound($"The product with the name {searchTerm} it doesn't exist");
            }

            var productDto = _mapper.Map<List<ProductDto>>(product);
            return Ok(productDto);
        }

        [HttpPatch("searchProductByName/{name}/{quantity}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            var product = _productRepository.BuyProduct(name, quantity);

            if (string.IsNullOrWhiteSpace(name) || quantity <= 0) {
                return BadRequest("Name product or quantity product are not valid");
            }

            var foundProduct = _productRepository.ProductExists(name);
            if (!foundProduct) {
                return NotFound($"Product with {name} it doesn't exist");    
            }

            if (!_productRepository.BuyProduct(name, quantity)) { 
                ModelState.AddModelError("CustomerError", $"It couldn't buy product {name} the quantity requested is not available in stock");
                return BadRequest(ModelState);
            }

            var units = quantity == 1 ? "unit" : "units"; 
            return Ok($"It bought {quantity} {units} of the product '{name}'");
        }

        [HttpPut("productId:int", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("Customer", "The product it doesn't exist");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("Customer", $"The category with {updateProductDto.CategoryId} is already exist");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;
            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("Customer", $"Something went wrong saving register {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId == 0)
            {
                return BadRequest(ModelState); 
            }

            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"The product with {productId} it doesn't exist");
            }

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("Customer", $"Something went wrong saving register {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
