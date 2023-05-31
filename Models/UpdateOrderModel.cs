namespace YourWear_backend.Models;

public class UpdateOrderModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public string EditableObject { get; set; }
    public int Cost { get; set; }
}