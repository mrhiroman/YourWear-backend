namespace YourWear_backend.Models;

public class WearModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public ClothType ClothType { get; set; }
    public int CreatorId { get; set; }
    public string CreatorName { get; set; }
}