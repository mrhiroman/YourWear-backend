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
    public async Task<IActionResult> GetOrders()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
        if (user != null)
        {
            var orders = await _dbContext.Orders.Where(x => x.User.Id == user.Id).ToListAsync();
            return Json(orders.Select(x => new OrderModel
            {
                ClothType = x.ClothType,
                ImageUrl = x.ImageUrl,
                Cost = x.Cost,
                CreatorId = x.User.Id,
                CreatorName = x.User.Name,
                Id = x.Id
            }));
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
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
                        EditableObject = order.EditableObject,
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
        if (user != null)
        {
            await _dbContext.Orders.AddAsync(new Order
            {
                User = user,
                Cost = model.Cost,
                ClothType = model.ClothType,
                ImageUrl = model.ImageUrl,
                EditableObject = model.EditableObject,
                OrderStatus = OrderStatus.Draft
            });
            await _dbContext.SaveChangesAsync();
            return Ok(Json(model));
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet("undraft/{id}")]
    public async Task<IActionResult> PlaceOrder([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (user != null && order != null)
        {
            order.OrderStatus = OrderStatus.Placed;
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
            return Ok(Json(order));
        }
        
        return NotFound();
    }
}