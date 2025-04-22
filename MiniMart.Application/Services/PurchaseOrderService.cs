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

        public async Task<ServiceResponse<PurchaseResponse>> ProcessPurchaseOrderAsync(PurchaseRequest request)
        {
            MergeLineItems(request);

            decimal totalAmount = 0;
            List<(ProductInventory prodInv, int qty)> quantityToDeductTracker = new();

            foreach (var lineItem in request.LineItems)
            {
                var productInv = await _productInvRepository.GetByProductIdAsync(lineItem.ProductId);
                if (productInv is null)
                {
                    return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.NotFound, $"Product Id: {lineItem.ProductId} could not be found");
                }

                if (productInv.Quantity == 0 || productInv.Quantity < lineItem.Quantity)
                {
                    return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.InsufficientAmountStock, $"Insufficient stock for Product Id: {productInv.ProductId}");
                }

                totalAmount += lineItem.Quantity * productInv.Product.Price;
                quantityToDeductTracker.Add((productInv, lineItem.Quantity));
            }

            if (totalAmount == 0)
                return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.TotalAmountError, $"Total amount cannot be zero");

            if (totalAmount != request.TotalAmount)
                return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.TotalAmountError, $"Total amount calcuated must match total amount in request. Amount: {totalAmount}, request amount: {request.TotalAmount}");

            var paymentRequest = new PaymentRequest
            {
                CustomerId = request.CustomerId,
                PaymentMethod = "Paywith transfer",
                Amount = totalAmount
            };

            var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest);

            if (!paymentResponse.IsSuccessful)
                return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.PaymentGatewayError, paymentResponse.ErrorMessage);

            if (paymentResponse.TrackingReference is null)
                return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.PaymentGatewayError, "Something went wrong. pls try again");

            if (paymentRequest.Amount != paymentResponse.Amount)
                return ServiceResponse<PurchaseResponse>.Failure(ServiceCodes.TotalAmountError, "Amount mismatch error");

            var purchaseOrder = new PurchaseOrder
            {
                Amount = totalAmount,
                CustomerId = request.CustomerId,
                Date = DateTime.Now,
                OrderStatus = PurchaseStatus.Pending,
                TransactionReference = paymentResponse.TrackingReference,
                OrderData = JsonSerializer.Serialize(request)
            };

            quantityToDeductTracker.ForEach(x => x.prodInv.Quantity -= x.qty);

            var trxQuery = new TransactionQueryLog
            {
                LogDate = DateTime.Now,
                RefId = paymentResponse.TrackingReference,
                Status = TransactionStatus.Pending,
                Amount = totalAmount
            };

            await _purchaseOrderRepository.AddAsync(purchaseOrder);
            await _tsqLogRepository.AddAsync(trxQuery);
            await _unitOfWork.SaveChangesAsync();

            PurchaseResponse response = new PurchaseResponse
            {
                IsSuccessful = paymentResponse.IsSuccessful,
                BankName = paymentResponse.BankName,
                AccountNumber = paymentResponse.AccountNumber,
                Amount = totalAmount,
                CurrencyCode = paymentResponse.CurrencyCode,
                Message = "Pay into this account",
                TrackingReference = paymentResponse.TrackingReference,
                ExpiryTime = paymentResponse.ExpiryTime
            };

            return ServiceResponse<PurchaseResponse>.Success(response);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _purchaseOrderRepository.GetAllAsync();
        }

        public async Task<PurchaseOrderStatusResponse?> VerifyOrderStatusAsync(string referenceId)
        {
            var order = await _purchaseOrderRepository.GetByReferenceIdAsync(referenceId);
            if (order == null) return null;

            return order.OrderStatus switch
            {
                PurchaseStatus.Success => new PurchaseOrderStatusResponse() { isSuccessful = true, Message = "Purchase Order Processed Successfully", Staus = TransactionStatus.Processed },
                PurchaseStatus.Pending => new PurchaseOrderStatusResponse() { isSuccessful = false, Message = nameof(PurchaseStatus.Pending), Staus = TransactionStatus.Pending },
                _ => new PurchaseOrderStatusResponse() { isSuccessful = false, Message = nameof(PurchaseStatus.Failed), Staus = TransactionStatus.Failed }
            };
        }

        public async Task<ServiceResponse> ProcessOrderTransactionStatus(string refId, bool isSuccessful)
        {
            var order = await _purchaseOrderRepository.GetByReferenceIdAsync(refId);

            if (order is null) return ServiceResponse.Failure(ServiceCodes.NotFound, $"Order with refId '{refId}' could not be found");

            if (order.OrderStatus != PurchaseStatus.Pending) return ServiceResponse.Success();

            order.OrderStatus = isSuccessful ? PurchaseStatus.Success : PurchaseStatus.Failed;

            var tsq = await _tsqLogRepository.GetByReferenceIdAsync(refId);
            if (tsq is not null && tsq.Status == TransactionStatus.Pending)
            {
                tsq.Status = isSuccessful ? TransactionStatus.Processed : TransactionStatus.Failed;
                tsq.StatusMessage = "Updated by Webhook";
            }

            await _unitOfWork.SaveChangesAsync();
            return ServiceResponse.Success();
        }

        private static void MergeLineItems(PurchaseRequest request)
        {

            request.LineItems = request.LineItems
                            .GroupBy(x => x.ProductId)
                            .Select(g => new PurchaseItem { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                            .ToList();
        }
    }
}
