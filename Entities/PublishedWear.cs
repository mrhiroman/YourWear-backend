using System.ComponentModel.DataAnnotations.Schema;
using YourWear_backend.Models;

namespace YourWear_backend.Entities;

public class PublishedWear
{
    public int Id { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    [ForeignKey("EditableObjectId")]
    public virtual EditableObject EditableObject { get; set; }
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }
    public bool IsAdmin { get; set; }
}