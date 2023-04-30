namespace YourWear_backend.Models;

public class OrderModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public int Cost { get; set; }
    public ClothType ClothType { get; set; }
    public int CreatorId { get; set; }
    public string CreatorName { get; set; }
}