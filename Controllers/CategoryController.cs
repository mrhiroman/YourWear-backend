using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourWear_backend.Models;

namespace YourWear_backend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : Controller
{
    private readonly AppDbContext _dbContext;

    public CategoryController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _dbContext.Categories.OrderBy(c => c.Id).ToListAsync();
        return Json(categories.Select(c => c.Name));
    }
}