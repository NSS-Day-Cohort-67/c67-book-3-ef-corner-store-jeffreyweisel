using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    [Required]
    public int CashierId { get; set; }
    public Cashier Cashier { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public List<OrderProduct> OrderProducts { get; set; }
    public decimal? Total
    {
        get
        {
            decimal? totalPrice = OrderProducts?.Sum(op => op?.Product?.Price * op?.Quantity) ?? 0;
            return totalPrice;
        }
    }
}