using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using YourWear_backend.Entities;
using YourWear_backend.Models;

namespace YourWear_backend.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController: Controller
{
    private readonly AppDbContext _dbContext;

    public OrderController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderModel>), StatusCodes.Status200OK)]
    [SwaggerResponseHeader(200,"X-Total-Count", "integer", "Total object count. Use it for pagination!", "int32")]
    public async Task<IActionResult> GetOrders(
        [FromQuery(Name = "page")] int page = -1,
        [FromQuery(Name = "limit")] int limit = -1,
        [FromQuery(Name = "category")] string category = "")
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var dbCategory = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Name == category);
        
        if (user != null)
        {
            var orders = await (dbCategory != null
                ?_dbContext.Orders.Where(x => x.User.Id == user.Id && x.Category == dbCategory).ToListAsync() 
                : _dbContext.Orders.Where(x => x.User.Id == user.Id).ToListAsync());
            
            Response.Headers.Add("X-Total-Count", orders.Count.ToString());
            
            var mappedData = page == -1 ?
                orders : limit > 0 ? 
                    orders.Skip((page - 1) * limit).Take(limit) : Array.Empty<Order>();

            return Json(mappedData.Select(x => new OrderModel
            {
                Category = new CategoryModel {Name = x.Category.Name},
                ImageUrl = x.ImageUrl,
                Cost = x.Cost,
                CreatorId = x.User.Id,
                CreatorName = x.User.Name,
                Id = x.Id,
                OrderStatus = x.OrderStatus
            }));
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (user != null && order != null)
        {
            switch (order.OrderStatus)
            {
                case OrderStatus.Draft:
                    return Json(new OrderModel
                    {
                        Category = new CategoryModel {Name = order.Category.Name},
                        ImageUrl = order.ImageUrl,
                        OrderStatus = OrderStatus.Draft,
                        Cost = order.Cost,
                        CreatorId = user.Id,
                        CreatorName = user.Name,
                        Id = order.Id
                    });
                case OrderStatus.Placed:
                    return Json(new OrderModel
                    {
                        Category = new CategoryModel {Name = order.Category.Name},
                        ImageUrl = order.ImageUrl,
                        OrderStatus = OrderStatus.Placed,
                        Cost = order.Cost,
                        CreatorId = user.Id,
                        CreatorName = user.Name,
                        Id = order.Id
                    });
            }
        }

        return NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SaveOrder([FromBody] AddOrderModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Name == model.Category);
        if (user != null && category != null)
        {
            var order = new Order
            {
                User = user,
                Cost = model.Cost,
                Category = category,
                ImageUrl = model.ImageUrl,
                OrderStatus = OrderStatus.Draft,
                EditableObject = new EditableObject
                {
                    ObjectValue = model.EditableObject
                }
            };
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
            return Ok(Json(model));
        }

        return BadRequest();
    }
    
    [Authorize]
    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> SaveOrder([FromBody] UpdateOrderModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == model.Id);
        if (user != null && order != null)
        {
            order.ImageUrl = model.ImageUrl;
            order.Cost = model.Cost;
            order.EditableObject = new EditableObject {ObjectValue = model.EditableObject};
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
            return Ok(Json(model));
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet("undraft/{id}")]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> PlaceOrder([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (user != null && order != null)
        {
            order.OrderStatus = OrderStatus.Placed;
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
            return Ok(Json(order.OrderStatus));
        }
        
        return NotFound();
    }

    [Authorize]
    [HttpGet("getobject/{id}")]
    public async Task<IActionResult> GetEditableObject([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (user != null && order != null)
        {
            if (order.OrderStatus == OrderStatus.Draft)
            {
                return Ok(Json(order.EditableObject.ObjectValue));
            }

            return BadRequest(new {ErrorText = "Cannot get Editable object for Published order."});
        }
        
        return NotFound();
    }

}