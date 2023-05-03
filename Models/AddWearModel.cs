namespace YourWear_backend.Models;

public class AddWearModel
{
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string EditableObject { get; set; }
    public ClothType ClothType { get; set; }
}