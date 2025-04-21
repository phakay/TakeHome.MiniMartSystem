using MiniMart.Application.Models;
using MiniMart.Common;
using MiniMart.Domain.Models;

public class Program
{
    private static readonly ApiService ApiService = new (new HttpClient());
    private static List<ProductInventoryResponse> _goods = new();
    private static string _lastPaymentRef = string.Empty;
    
    public static void Main()
    {
        var shouldExit = false;
        while (!shouldExit)
        {
            Console.WriteLine("\nMini Mart Console App");
            Console.WriteLine("1. View Available Goods");
            Console.WriteLine("2. Purchase Goods");
            Console.WriteLine("3. Check Payment Status");
            Console.WriteLine("4. Exit");
            Console.WriteLine();

            Console.WriteLine("Input a number from the option list to access the menu");
            var input = Console.ReadLine();

            try
            {
                switch (input)
                {
                    case "1":
                        Console.WriteLine("=== View Available Goods ===");
                        ViewGoods(); break;
                    case "2":
                        Console.WriteLine("=== Purchase Goods ===");
                        PurchaseGoods(); break;
                    case "3":
                        Console.WriteLine("=== Check Payment Status ===");
                        CheckPaymentStatus(); break;
                    case "4":
                        Console.WriteLine("Exiting...");
                        shouldExit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option selection. Try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred. Please Try again");
                Console.WriteLine("Error Message: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }
    }

    private static void CheckPaymentStatus()
    {
        // Checking payment status
        if (string.IsNullOrEmpty(_lastPaymentRef))
        {
            Console.WriteLine("No record found");
            return;
        }

        Console.WriteLine("Checking for transaction Reference: " + _lastPaymentRef);
        var reconfirmResponse = ApiService.VerifyPurchaseStatus(_lastPaymentRef);

        if (reconfirmResponse is null)
        {
            Console.WriteLine("An error occurred...");
            return;
        }

        if (reconfirmResponse.isSuccessful)
        {
            Console.WriteLine("Payment Successful");
            return;
        }

        if (reconfirmResponse.Staus == TransactionStatus.Failed)
        {
            Console.WriteLine("Order Failed");
            return;
        }

        if (reconfirmResponse.Staus == TransactionStatus.Pending)
        {
            Console.WriteLine("Payment is still not confirmed");
            return;
        }
    }

    /// <summary>
    /// Method ensures a valid selection is made. 
    /// Either a valid selection is made or the system continues to prompt for a selection. 
    /// The loop can be broken by fulfilling the conditoin for the <paramref name="isBreakOutCondition"/> 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="validatorFunc"></param>
    /// <param name="isBreakOutCondition"></param>
    /// <returns></returns>
    private static (string result, bool isBreakOut) InputProcessor(
        string msg,
        Func<string, (bool isValid, string? errorMsg)> validatorFunc, 
        Predicate<string>? isBreakOutCondition = null, string breakOutKey = "exit")
    {
        if (isBreakOutCondition == null)
            isBreakOutCondition = i => string.Equals(i.Trim(), breakOutKey, StringComparison.OrdinalIgnoreCase);

        while (true)
        {
            Console.WriteLine(msg);
            var input = Console.ReadLine();
            if (input is null) continue;

            if (isBreakOutCondition(input))
            {
                return (string.Empty, true);
            }

            var valResult = validatorFunc(input);
            if (valResult.isValid)
            {
                return (input, false);
            }

            if (!string.IsNullOrEmpty(valResult.errorMsg))
                Console.WriteLine("Invalid input. Try again." + valResult.Item2);
            else
                Console.WriteLine("Invalid input. Try again.");

            Console.WriteLine($"Type '{breakOutKey}' to exit at any time");
        }
    }
    
    private static void PurchaseGoods()
    {
        _goods = ApiService.GetGoods();
        if (_goods.Count == 0)
        {
            Console.WriteLine("No Available goods to purchase at the moment.");
            return;
        }

        var goods = _goods.ToDictionary(x => x.ProductId);
        var userIdInput = InputProcessor("Enter you name", i => !string.IsNullOrEmpty(i) ? (true, null) : (false, string.Empty));
        if (userIdInput.isBreakOut) return;
        var userId = userIdInput.result;
        
        var orderItems = new List<PurchaseItem>();
        Console.WriteLine("\nBegin Selecting an item from the list");

        while (true)
        {
            var itemIdInput = InputProcessor("\nEnter Item ID", i =>
            {
                if (string.IsNullOrEmpty(i) || !int.TryParse(i, out var value))
                    return (false, string.Empty);

                if (!_goods.Any(x => x.ProductId == value))
                    return (false, "Item ID is not in the list");
                return (true, i);
            });

            if (itemIdInput.isBreakOut) break;
            var itemId = int.Parse(itemIdInput.result);

            var itemQtyInput = InputProcessor("\nEnter Quantity", i =>
            {
                if (string.IsNullOrEmpty(i) || !int.TryParse(i, out var qty))
                    return (false, string.Empty);

                if (qty > goods[itemId].Quantity)
                    return (false, "\nItem stock is not sufficient for the input quantity. " +
                                   $"Remaining stock is '{goods[itemId].Quantity}'");

                goods[itemId].Quantity -= qty;
                return (true, i);
            });
            if (itemQtyInput.isBreakOut) break;
            var qty = int.Parse(itemQtyInput.result);

            orderItems.Add(new PurchaseItem { ProductId = itemId, Quantity = qty });

            Console.WriteLine("\nItem Added to your order list");

            var toContinueInput = InputProcessor("\nDo you want to add more items to your order list? (yes or no)", i =>
            {
                if (string.IsNullOrEmpty(i))
                    return (false, string.Empty);

                if (!string.Equals(i, "yes", StringComparison.OrdinalIgnoreCase) && !string.Equals(i, "no", StringComparison.OrdinalIgnoreCase))
                    return (false, "Either 'yes' or 'no' is allowed.");

                return (true, null);
            });

            if (toContinueInput.isBreakOut || string.Equals(toContinueInput.result, "no", StringComparison.OrdinalIgnoreCase)) break;
        }
        
        if (orderItems.Count == 0)
        {
            Console.WriteLine("\nNo item added to your Order");
            return;
        }

        Console.WriteLine("\n========================================= Your Order List  =======================================");

        decimal totalAmount = 0;
        foreach (var order in orderItems)
        {
            var product = goods[order.ProductId];
            Console.WriteLine($"ID: {order.ProductId} | Name: {product.Name} | Unit Price: {product.Price} | Qty {order.Quantity} | Total Price: {order.Quantity * product.Price}");
            totalAmount += order.Quantity * product.Price;
        }
        Console.WriteLine("Total amount: " + totalAmount);

        var toSendOrderInput = InputProcessor("\nDo you want to send order and initiate payment. (yes or no. 'no' will take you back to main menu)", i =>
        {
            if (string.IsNullOrEmpty(i))
                return (false, string.Empty);

            if (!string.Equals(i, "yes", StringComparison.OrdinalIgnoreCase) && !string.Equals(i, "no", StringComparison.OrdinalIgnoreCase))
                return (false, "Either 'yes' or 'no' is allowed.");

            return (true, null);
        });

        var toSendOrder = toSendOrderInput.result.ToLower();
        if (toSendOrder == "no") return;

        // call purchase endpoint
        var orderData = new PurchaseRequest { CustomerId = userId, LineItems = orderItems, TotalAmount = totalAmount };
        var result = ApiService.MakeOrder(orderData);

        if (!result.IsSuccessful)
        {
            Console.WriteLine(result.Message);
            return;
        }

        _lastPaymentRef = result.TrackingReference;

        Console.WriteLine("\nAccount details to pay into. Please pay the exact amount into the account before the account expires");
        Console.WriteLine("Bank Name: " + result.BankName);
        Console.WriteLine("Account No: " + result.AccountNumber);
        Console.WriteLine("Amount: " + result.Amount);
        Console.WriteLine("Currency: " + result.CurrencyCode);
        Console.WriteLine("Expires " + result.ExpiryTime.ToString("yyyy-MM-dd hh:mm:ss"));
       
        Console.WriteLine("\nConfirming Pay with Transfer Status...");
        // polls the api for 30sec
        int retryCount = 3;
        do
        {
            // awaiting confirmation...
            var reconfirmResponse = ApiService.VerifyPurchaseStatus(_lastPaymentRef);
            if (reconfirmResponse == null) continue;

            if (reconfirmResponse.isSuccessful)
            {
                Console.WriteLine("Order Complete");
                break;
            }
            else if (reconfirmResponse.Staus == TransactionStatus.Failed)
            {
                Console.WriteLine("Order Failed");
                break;
            }
            else
            {
                // back off and retry
                Task.Delay(5000).Wait();
            }
        } while (--retryCount > 0);

        if (retryCount == 0)
        {
            Console.WriteLine($"\nUnable to Confirm transaction at this time. Please check back later. RefId: {_lastPaymentRef}");
        }
    }

    private static void ViewGoods()
    {
        _goods = ApiService.GetGoods();
        if (_goods.Count == 0)
        {
            Console.WriteLine("\nNo Data to display");
            return;
        }

        Console.WriteLine("\nGoods List: ");
        foreach (var item in _goods)
        {
            Console.WriteLine($"ID: {item.ProductId} | Name: {item.Name} | Price: {item.Price} | Stock: {item.Quantity}");
        }
    }
}


public class ApiService(HttpClient httpClient) : BaseApiClient(httpClient)
{
    private const string BaseUrl = "https://localhost:7222/api/";
    
    public List<ProductInventoryResponse> GetGoods()
    {
        return GetAsync<List<ProductInventoryResponse>>($"{BaseUrl}inventory/getavailableproducts").Result;
    }

    public PurchaseResponse MakeOrder(PurchaseRequest request)
    {
        return PostAsync<PurchaseResponse>($"{BaseUrl}purchases/makeorder", request).Result;
    }

    public PurchaseOrderStatusResponse VerifyPurchaseStatus(string refId)
    {
        return GetAsync<PurchaseOrderStatusResponse>($"{BaseUrl}purchases/verifyorderstatus/{refId}").Result;
    }
}