namespace YourWear_backend.Models;

public class AddOrderModel
{
    public string ImageUrl { get; set; }
    public int Cost { get; set; }
    public ClothType ClothType { get; set; }
}