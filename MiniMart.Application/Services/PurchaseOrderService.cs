using System.Text.Json;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IProductInventoryRepository _productInvRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly ITransactionQueryLogRepository _tsqLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalGatewayPaymentService _paymentService;

        public PurchaseOrderService(
            IProductInventoryRepository productInvRepository, 
            IPurchaseOrderRepository purchaseRepository, 
            ITransactionQueryLogRepository tsqLogRepository,
            IUnitOfWork unitOfWork, 
            IExternalGatewayPaymentService paymentService)
        {
            _productInvRepository = productInvRepository;
            _purchaseOrderRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _tsqLogRepository = tsqLogRepository;
        }

        public async Task<PurchaseResponse> ProcessPurchaseOrderAsync(PurchaseRequest request)
        {
            MergeLineItems(request);

            decimal totalAmount = 0;
            List<(ProductInventory, int)> quantityToDeductTracker = new();

            foreach (var lineItem in request.LineItems)
            {
                var productInv = await _productInvRepository.GetByProductIdAsync(lineItem.ProductId);
                if (productInv is null)
                {
                    return new PurchaseResponse { IsSuccessful = false, Message = $"Product Id: {lineItem.ProductId} could not be found" };
                }

                if (productInv.Quantity == 0 || productInv.Quantity < lineItem.Quantity)
                {
                    return new PurchaseResponse { IsSuccessful = false, Message = $"Insufficient stock for Product Id: {productInv.ProductId}" };
                }

                totalAmount += lineItem.Quantity * productInv.Product.Price;
                quantityToDeductTracker.Add((productInv, lineItem.Quantity));
            }

            if (totalAmount == 0)
                return new PurchaseResponse { IsSuccessful = false, Message = $"Total amount cannot be zero" };

            if (totalAmount != request.TotalAmount)
                return new PurchaseResponse { IsSuccessful = false, Message = $"Total amount calcuated must match total amount in request. Amount: {totalAmount}, request amount: {request.TotalAmount}" };

            var trxRef = Guid.NewGuid().ToString();
            var purchaseOrder = new PurchaseOrder
            {
                Amount = totalAmount,
                CustomerId = request.CustomerId,
                Date = DateTime.Now,
                OrderStatus = PurchaseStatus.Pending,
                TransactionReference = trxRef,
                OrderData = JsonSerializer.Serialize(request)
            };

            await _purchaseOrderRepository.AddAsync(purchaseOrder);

            quantityToDeductTracker.ForEach(x => x.Item1.Quantity -= x.Item2);

            var paymentRequest = new PaymentRequest
            {
                TransactionReference = trxRef,
                CustomerId = request.CustomerId,
                PaymentMethod = "Paywith transfer",
                MerchantId = "MerchantId",
                Amount = totalAmount
            };

            var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest);

            if (!paymentResponse.IsSuccess)
                return new PurchaseResponse { IsSuccessful = false, Message = "Unable to process payment method" };

            var trxQuery = new TransactionQueryLog
            {
                LogDate = DateTime.Now,
                RefId = trxRef,
                Status = TransactionStatus.Pending
            };

            await _tsqLogRepository.AddAsync(trxQuery);

            await _unitOfWork.SaveChangesAsync();

            PurchaseResponse response = new PurchaseResponse
            {
                IsSuccessful = paymentResponse.IsSuccess,
                AccountNumber = paymentResponse.AccountNumber,
                Amount = totalAmount,
                CurrencyCode = paymentResponse.CurrencyCode,
                Message = "Pay into this account",
                TransactionReference = trxRef
            };

            return response;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _purchaseOrderRepository.GetAllAsync();
        }

        public async Task<PurchaseOrderStatusResponse?> VerifyOrderStatusAsync(string referenceId)
        {
            var order = await _purchaseOrderRepository.GetByReferenceId(referenceId);
            if (order == null) return null;

            return order.OrderStatus switch
            {
                PurchaseStatus.Success => new PurchaseOrderStatusResponse() { isSuccessful = true, Message = "Purchase Order Processed Successfully" },
                PurchaseStatus.Pending => new PurchaseOrderStatusResponse() { isSuccessful = false, Message = nameof(PurchaseStatus.Pending) },
                _ => new PurchaseOrderStatusResponse() { isSuccessful = false, Message = nameof(PurchaseStatus.Failed) }
            };
        }

        private static void MergeLineItems(PurchaseRequest request)
        {

            request.LineItems = request.LineItems
                            .GroupBy(x => x.ProductId)
                            .Select(g => new PurchaseItem { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                            .ToList();
        }

        public async Task ProcessOrderTransactionStatus(string refId, bool isSuccessful)
        {
            var order = await _purchaseOrderRepository.GetByReferenceId(refId);

            if (order is null) throw new ApplicationException($"Order with refId '{refId}' could not be found");

            if (order.OrderStatus != PurchaseStatus.Pending) return;

            order.OrderStatus = isSuccessful ? PurchaseStatus.Success : PurchaseStatus.Failed;   

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
