namespace CornerStore.Models.DTOs;

public class CashierDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName 
    {
        get
        {
            string fullName = $"{FirstName} {LastName}";
            return fullName;
        }
    }
    public List<OrderDTO> Orders { get; set; }

}