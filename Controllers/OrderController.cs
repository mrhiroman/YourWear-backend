using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> GetOrders(
        [FromQuery(Name = "page")] int page = -1,
        [FromQuery(Name = "limit")] int limit = -1)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        
        if (user != null)
        {
            var orders = await _dbContext.Orders.Where(x => x.User.Id == user.Id).ToListAsync();
            
            Response.Headers.Add("X-Total-Count", orders.Count.ToString());
            
            var mappedData = page == -1 ?
                orders : limit > 0 ? 
                    orders.Skip((page - 1) * limit).Take(limit) : Array.Empty<Order>();
            
            return Json(mappedData.Select(x => new OrderModel
            {
                ClothType = x.ClothType,
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
                        ClothType = order.ClothType,
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
                        ClothType = order.ClothType,
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
        if (user != null)
        {
            var order = new Order
            {
                User = user,
                Cost = model.Cost,
                ClothType = model.ClothType,
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