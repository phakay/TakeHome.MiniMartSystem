using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly IMapper _mapper;

        public ProductsController(IProductService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [Produces<ProductResponse>]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _service.GetProductByIdAsync(id);

            if (product is null) 
                return NotFound();

            return Ok(_mapper.Map<ProductResponse>(product));
        }

        /// <summary>
        /// List Goods
        /// </summary>
        /// <returns><see cref="ProductResponse"/></returns>
        [HttpGet]
        [Produces<IEnumerable<ProductResponse>>]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetProductsAsync();

            return Ok(_mapper.Map<IEnumerable<ProductResponse>>(products));
        }

        /// <summary>
        /// Create Goods
        /// </summary>
        /// <param name="productRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductCreateRequest productRequest)
        {
            if (await _service.DoesProductNameExistsAsync(productRequest.Name))
                return Conflict(new { errorMessage = "Product name already exists" });

            var product = _mapper.Map<Product>(productRequest);
            
            await _service.AddProductAsync(product);

            var productResponse = _mapper.Map<ProductResponse>(product);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, productResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] ProductUpdateRequest productRequest, int id)
        {
            if (id != productRequest.Id)
                return BadRequest("id in the url must match id in the request body"); 

            if (!await _service.DoesProductIdExistsAsync(id))
                return NotFound();

            if (!await _service.DoesProductNameExistsAsync(productRequest.Name))
                return Conflict(new { errorMessage = "Product name already exists" });

            var product = _mapper.Map<Product>(productRequest);

            await _service.UpdateProductAsync(product);

            var productResponse = _mapper.Map<ProductResponse>(product);

            return Ok(productResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _service.DoesProductIdExistsAsync(id))
                return NotFound();

            await _service.RemoveProductAsync(id);

            return NoContent();
        }
    }
}
