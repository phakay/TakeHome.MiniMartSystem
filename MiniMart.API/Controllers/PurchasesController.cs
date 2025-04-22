using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchasesController : BaseController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IMapper _mapper;
        public PurchasesController(IPurchaseOrderService purchaseOrderService, IMapper mapper)
        {
            _purchaseOrderService = purchaseOrderService;
            _mapper = mapper;
        }

        /// <summary>
        /// Purchase Goods
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("makeorder")]
        public async Task<IActionResult> Purchase([FromBody] PurchaseRequest request)
        {
            var response = await _purchaseOrderService.ProcessPurchaseOrderAsync(request);
            if (response.ResponseCode == ServiceCodes.Success)
             return CreateCustomResult(HttpStatusCode.OK, response);
            return CreateCustomResult(HttpStatusCode.BadRequest, $"{response.ResponseCode}-{response.ErrorMessage}");
        }

        [HttpGet("getorders")]
        public async Task<IActionResult> GetOrders()
        {
            var response = await _purchaseOrderService.GetPurchaseOrdersAsync();
            return CreateCustomResult(HttpStatusCode.OK, _mapper.Map<IEnumerable<PurchaseOrderResponse>>(response));
        }

        [HttpGet("verifyorderstatus/{referenceId}")]
        public async Task<IActionResult> Verify(string referenceId)
        {
            var response = await _purchaseOrderService.VerifyOrderStatusAsync(referenceId);
            if (response == null) return CreateCustomResult(HttpStatusCode.NotFound);

            return CreateCustomResult(HttpStatusCode.OK, response);
        }
    }
}
