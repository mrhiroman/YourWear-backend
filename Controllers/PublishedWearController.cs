using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourWear_backend.Entities;
using YourWear_backend.Models;

namespace YourWear_backend.Controllers;

[ApiController]
[Route("/api/wears")]
[Description]
public class PublishedWearController : Controller
{
    private readonly AppDbContext _dbContext;

    public PublishedWearController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWears()
    {
        var wears = await _dbContext.PublishedWears.Where(x => x.User != null).ToListAsync();
        return Json(wears.Select(x => new WearModel
        {
            ClothType = x.ClothType,
            Name = x.Name,
            ImageUrl = x.ImageUrl, 
            CreatorId = x.User.Id,
            CreatorName = x.User.Name,
            Id = x.Id
        }));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        var wear = await _dbContext.PublishedWears.FirstOrDefaultAsync(x => x.Id == id);
        if (wear != null)
        {
            return Json(new WearModel
            {
                ClothType = wear.ClothType,
                ImageUrl = wear.ImageUrl,
                CreatorId = wear.User.Id,
                CreatorName = wear.User.Name,
                Name = wear.Name,
                Id = wear.Id
            });
        }

        return NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] AddWearModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
        if (user != null)
        {
            await _dbContext.PublishedWears.AddAsync(new PublishedWear
            {
                User = user,
                ClothType = model.ClothType,
                ImageUrl = model.ImageUrl,
                Name = model.Name
            });
            await _dbContext.SaveChangesAsync();
            return Ok(Json(model));
        }

        return BadRequest();
    }
    
    
}