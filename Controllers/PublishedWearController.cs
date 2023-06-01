using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
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
    [SwaggerResponseHeader(200,"X-Total-Count", "integer", "Total object count. Use it for pagination!", "int32")]
    public async Task<IActionResult> GetAllWears(
        [FromQuery(Name = "page")] int page = -1,
        [FromQuery(Name = "limit")] int limit = -1,
        [FromQuery(Name = "category")] string category = "")
    {
        var dbCategory = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Name == category);
        var wears = await (dbCategory != null
            ? _dbContext.PublishedWears.Where(x => x.User != null && x.Category == dbCategory).ToListAsync() 
            : _dbContext.PublishedWears.Where(x => x.User != null).ToListAsync());
        
        Response.Headers.Add("X-Total-Count", wears.Count.ToString());
            
        var mappedData = page == -1 ?
            wears : limit > 0 ? 
                wears.Skip((page - 1) * limit).Take(limit) : Array.Empty<PublishedWear>();
        
        return Json(mappedData.Select(x => new WearModel
        {
            Category = new CategoryModel {Name = x.Category.Name},
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
                Category = new CategoryModel {Name = wear.Category.Name},
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
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == model.OrderId);
        if (user != null && order != null)
        {
            var wear = new PublishedWear
            {
                User = user,
                Category = order.Category,
                ImageUrl = order.ImageUrl,
                Name = model.Name,
                EditableObject = new EditableObject
                {
                    ObjectValue = order.EditableObject.ObjectValue
                }
            };
            await _dbContext.PublishedWears.AddAsync(wear);
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
                return Ok(Json(wear.EditableObject.ObjectValue));
        }
        
        return NotFound();
    }

    [HttpGet]
    [Route("featured")]
    [ProducesResponseType(typeof(IEnumerable<WearModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeaturedWears()
    {
        var featuredWears = await _dbContext.PublishedWears.Where(w => w.IsAdmin == true).ToListAsync();
        Response.Headers.Add("X-Total-Count", featuredWears.Count.ToString());
        return Json(featuredWears.Select(x => new WearModel
        {
            Category = new CategoryModel {Name = x.Category.Name},
            Name = x.Name,
            ImageUrl = x.ImageUrl,
            Id = x.Id
        }));
    }
    
}