namespace CornerStore.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public int CashierId { get; set; }
    public CashierDTO Cashier { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; }
    public decimal? Total
    {
        get
        {
            decimal totalPrice = OrderProducts.Sum(op => op.Product.Price * op.Quantity);
            return totalPrice;
        }
    }
}