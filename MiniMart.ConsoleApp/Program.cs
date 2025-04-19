
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MiniMart.Application.Models;

public class Program
{
    private static HttpClient _httpClient = new HttpClient();
    private static JsonSerializerOptions jsonOption => new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    private static List<ProductResponse> _goods = new();
    private static string _lastPaymentRef = string.Empty;
    public static void Main()
    {
        while (true)
        {
            Console.WriteLine("\nMini Mart Console App");
            Console.WriteLine("1. View Available Goods");
            Console.WriteLine("2. Purchase Goods");
            Console.WriteLine("3. Check Payment Status");
            Console.WriteLine("4. Exit");
            Console.WriteLine();

            var input = Console.ReadLine();
            Console.WriteLine();
            try
            {
                switch (input)
                {
                    case "1":
                        ViewGoods();
                        break;
                    case "2":
                        PurchaseGoods();
                        break;
                    case "3":
                        CheckPaymentStatus();
                        break;
                    case "4":
                        return;
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
        }
    }

    private static void CheckPaymentStatus()
    {
        // Checking payment status
        Console.WriteLine("Checking for trxRef: " + _lastPaymentRef);
        var reconfirmResponse = _httpClient.GetStringAsync(Constants.ApiUrl + $"purchases/verify/{_lastPaymentRef}").Result;
        var dataResponse = JsonSerializer.Deserialize<OrderStatusResponse>(reconfirmResponse, jsonOption);
        if (dataResponse is not null && dataResponse.isSuccessful)
        {
            Console.WriteLine("Payment Successful");
        }
        else
        {
            Console.WriteLine("Payment is still not confirmed");
        }
    }

    private static void PurchaseGoods()
    {
        var hashedGoods = _goods.ToDictionary(x => x.Id);
        Console.WriteLine("Enter Customer Name");
        var userId = Console.ReadLine();
        var orderItems = new List<PurchaseItem>();
        while (true)
        {
            Console.WriteLine("Enter Item ID");
            Console.WriteLine("Type 'main' at anytime to go back to main menu");
            Console.WriteLine("Type 'done' after completing the entries");
            var input = Console.ReadLine();
            if (input == "done") break;
            if (input == "main") return;

            if (!int.TryParse(input, out var itemId))
            {
                Console.WriteLine("Invalid Item ID");
                continue;
            }

            if (!_goods.Any(x => x.Id == itemId))
            {
                Console.WriteLine("Item ID is not in the list");
                continue;
            }

            int itemQty = 0;
            while (true)
            {
                Console.WriteLine("Type 'main' at anytime to go back to main menu");
                Console.WriteLine("Type 'done' after completing the entries");
                Console.WriteLine("Enter Quantity");
                var qtyInput = Console.ReadLine();
                if (qtyInput == "done") break;
                if (qtyInput == "main") return;
                if (int.TryParse(qtyInput, out var qty))
                {
                    if (qty > hashedGoods[itemId].Quantity)
                    {
                        Console.WriteLine("Insufficient Quantity");
                        continue;
                    }
                    else
                    {
                        itemQty = qty;
                        hashedGoods[itemId].Quantity -= qty;
                        break;
                    }
                }
                Console.WriteLine("Invalid Qty Input");
            }

            if (itemQty > 0)
            {
                orderItems.Add(new PurchaseItem() { ProductId = itemId, Quantity = itemQty });
            }
        }

        if (orderItems.Count == 0) return;


        Console.WriteLine("See Order...");

        decimal total = 0;
        foreach (var order in orderItems)
        {
            var product = hashedGoods[order.ProductId];
            Console.WriteLine($"ID: {order.ProductId} | Name: {product.Name} | Unit Price: {product.Price} | Qty {order.Quantity} | Total Price: {order.Quantity * product.Price}");
            total += order.Quantity * product.Price;
        }

        Console.WriteLine("Total amount: " + total);

        Console.ReadKey();

        var orderData = new PurchaseRequest { CustomerId = userId, LineItems = orderItems };
        var jsonContent = new StringContent(JsonSerializer.Serialize(orderData), Encoding.UTF8, "application/json");

        // call purchase api
        var response = _httpClient.PostAsync(Constants.ApiUrl + "purchases/makeorder", jsonContent).Result;
        var result = JsonSerializer.Deserialize<PurchaseResponse>(response.Content.ReadAsStringAsync().Result, jsonOption);
        _lastPaymentRef = result.TransactionReference;

        Console.WriteLine("Account details: " + result.AccountNumber);
        Console.WriteLine("Amount: " + result.Amount);
        Console.WriteLine("Currency: " + result.CurrencyCode);
        
        // polls for 30sec
        int retryCount = 3;
        do
        {
            // awaiting confirmation...
            var reconfirmResponse = _httpClient.GetStringAsync(Constants.ApiUrl + $"purchases/verify/{result.TransactionReference}").Result;
            var dataResponse = JsonSerializer.Deserialize<OrderStatusResponse>(reconfirmResponse, jsonOption);
            if (dataResponse is not null && dataResponse.isSuccessful)
            {
                Console.WriteLine("Order Complete");
                break;
            }
            else
            {
                // back off and retry
                Task.Delay(5000).Wait();
            }
        } while (retryCount-- > 0);

        if (retryCount == 0)
        {
            Console.WriteLine("Unable to Confirm transaction");
        }
    }

    private static void ViewGoods()
    {
        var response = _httpClient.GetStringAsync(Constants.ApiUrl + "goods").Result;
        var result = JsonSerializer.Deserialize<List<ProductResponse>>(response, jsonOption);
        _goods = result ?? [];
        if (_goods.Count == 0)
        {
            Console.WriteLine("No Data to display");
        }

        foreach (var item in _goods)
        {
            Console.WriteLine($"ID: {item.Id} | Name: {item.Name} | Price: {item.Price} | Stock: {item.Quantity}");
        }
    }
}


public static class Constants
{
    public const string ApiUrl = "https://localhost:7222/api/";
}

