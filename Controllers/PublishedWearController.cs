﻿using System.ComponentModel;
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
    [ProducesResponseType(typeof(IEnumerable<WearModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllWears(
        [FromQuery(Name = "page")] int page = -1,
        [FromQuery(Name = "limit")] int limit = -1)
    {
        var wears = await _dbContext.PublishedWears.Where(x => x.User != null).ToListAsync();
        Response.Headers.Add("X-Total-Count", wears.Count.ToString());
            
        var mappedData = page == -1 ?
            wears : limit > 0 ? 
                wears.Skip((page - 1) * limit).Take(limit) : Array.Empty<PublishedWear>();
        
        return Json(mappedData.Select(x => new WearModel
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
    [ProducesResponseType(typeof(WearModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWearById([FromRoute] int id)
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
    public async Task<IActionResult> AddWear([FromBody] AddWearModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        if (user != null)
        {
            await _dbContext.PublishedWears.AddAsync(new PublishedWear
            {
                User = user,
                ClothType = model.ClothType,
                ImageUrl = model.ImageUrl,
                EditableObject = model.EditableObject,
                Name = model.Name
            });
            await _dbContext.SaveChangesAsync();
            return Ok(Json(model));
        }

        return BadRequest();
    }
    
    [Authorize]
    [HttpGet("getobject/{id}")]
    public async Task<IActionResult> GetEditableObject([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var wear = await _dbContext.PublishedWears.FirstOrDefaultAsync(x => x.Id == id);
        if (user != null && wear != null)
        {
                return Ok(Json(wear.EditableObject));
        }
        
        return NotFound();
    }
    
}