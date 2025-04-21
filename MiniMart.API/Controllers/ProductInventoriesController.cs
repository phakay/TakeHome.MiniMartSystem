using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class ProductInventoriesController : BaseController
    {
        private readonly IProductInventoryService _productInvService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductInventoriesController(IProductInventoryService productInvService, IProductService productService, IMapper mapper)
        {
            _productInvService = productInvService;
            _productService = productService;
            _mapper = mapper;
        }
        
        [HttpGet("getproductinventory/{productId}")]
        public async Task<IActionResult> GetProductInventory(int productId)
        {
            var response = await _productInvService.GetProductInventoryByProductIdAsync(productId);

            return response is null ? CreateCustomResult(HttpStatusCode.NotFound, "No Inventory record for ProductId " + productId) : 
                CreateCustomResult(HttpStatusCode.OK,_mapper.Map<ProductInventoryResponse>(response));
        }

        [HttpGet("getavailableproducts")]
        public async Task<IActionResult> GetAvailableProducts()
        {
            var response = _mapper.Map<IEnumerable<ProductInventoryResponse>>(await _productInvService.GetAvailableProductsInStockAsync());  
            return CreateCustomResult(HttpStatusCode.OK, response);
        }

        [HttpGet("getproducts")]
        public async Task<IActionResult> GetAll()
        {
            var response = _mapper.Map<IEnumerable<ProductInventoryResponse>>(await _productInvService.GetAllProductInventoriesAsync());
            return CreateCustomResult(HttpStatusCode.OK, response);
        }

        [HttpPost("addinventory")]
        public async Task<IActionResult> AddToInventory(ProductInventoryRequest request)
        {
            if (!await _productService.DoesProductIdExistsAsync(request.ProductId))
                return CreateCustomResult(HttpStatusCode.NotFound, "The product Id could not be found");

            await _productInvService.AddQuantityToInventoryAsync(request.ProductId, request.Quantity);
            return CreateCustomResult(HttpStatusCode.OK);
        }

        [HttpPost("removeinventory")]
        public async Task<IActionResult> RemoveFromInventory(ProductInventoryRequest request)
        {
            if (!await _productService.DoesProductIdExistsAsync(request.ProductId))
                return CreateCustomResult(HttpStatusCode.NotFound, "The product Id could not be found");

            await _productInvService.RemoveQuantityFromInventory(request.ProductId, request.Quantity);
            return Ok();
        }
    }
}
