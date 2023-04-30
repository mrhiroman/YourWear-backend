﻿using System.ComponentModel.DataAnnotations.Schema;
using YourWear_backend.Models;

namespace YourWear_backend.Entities;

public class PublishedWear
{
    public int Id { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public ClothType ClothType { get; set; }
}