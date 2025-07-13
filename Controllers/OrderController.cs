using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceApp.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "User")]
    [HttpGet("api/ViewOrder")]
    public async Task<IActionResult> ViewOrder(int CartNo)
    {
        var OrderExist = await _context.Orders.AnyAsync(o => o.CartId == CartNo);
        if (!OrderExist)
        {
            return Ok("You order history will be shown here.");
        }
        
        var orderHistoryList = await _context.Orders.Where(o => o.CartId == CartNo)
                                        .Select(o => new { OrderId = o.Id, OrderCreatedAt = o.OrderDate, TotalAmount = o.TotalAmount, OrderStatus = o.OrderStatus })
                                        .ToListAsync();
        return Ok(orderHistoryList);
    }

    [Authorize(Roles = "User")]
    [HttpGet("api/ItemHistory")]
    public async Task<IActionResult> ItemHistory(int? OrderId)
    {
        if (!OrderId.HasValue)
        {
            return BadRequest("Please provide the order ID");
        }

        var OrderExist = await _context.Orders.Where(o => o.Id == OrderId).ToListAsync();
        if (!OrderExist.Any())
        {
            return BadRequest("There is no order placed yet");
        }

        var ItemList = await _context.Items
                                .Where(i => i.OrderId == OrderId && i.ItemStatus == "Ordered")
                                .Select(i => new { OrderId = i.OrderId, ProductName = i.Product.Name, Quantity = i.Quantity, TotalAmount = i.TotalAmount}).ToListAsync();
        
        return Ok(ItemList);
    }

    [Authorize(Roles = "User")]
    [HttpPost("api/AddOrder")]
    public async Task<IActionResult> AddOrder(int? CartId)
    {
        if (!CartId.HasValue)
        {
            return BadRequest("Please provide Cart ID");
        }
        var CartExist = await _context.Carts.AnyAsync(c => c.Id == CartId);
        if (!CartExist)
        {
            return Ok("Cart not found");
        }

        var CartItem = await _context.Items
                                    .Include(i => i.Product)
                                    .Where(i => i.CartId == CartId).ToListAsync();

        if (!CartItem.Any())
        {
            return Ok("Cart is empty");
        }

        var ItemInStock = await _context.Items
                                .Include(i => i.Product)
                                .Where(i => i.CartId == CartId && i.Quantity > i.Product.StockQuantity && i.OrderId == null)
                                .Select(i => new { ItemName = i.Product.Name, QuantityRequesed = i.Quantity, QuantityAvailable = i.Product.StockQuantity}).ToListAsync();

        if (ItemInStock.Any())
        {
            var message = "Some items exceed available stock:\n";
            foreach (var item in ItemInStock)
            {
                message += $"- {item.ItemName}: Requested = {item.QuantityRequesed}, Available = {item.QuantityAvailable}\n";
            }
            message += "Please edit the quantity and try again.";
            return BadRequest(message);
        }

        var TotalOrderPrice = await _context.Items.Where(i => i.CartId == CartId).SumAsync(i => i.TotalAmount);

        var NewOrder = new Order { CartId = CartId.Value, OrderDate = DateTime.Now, TotalAmount = TotalOrderPrice, OrderStatus = "Ordered" };
        await _context.Orders.AddAsync(NewOrder);
        await _context.SaveChangesAsync();

        var ItemStatus = await _context.Items.Where(i => i.CartId == CartId && i.OrderId == null).Include(i => i.Product).ToListAsync();
        ItemStatus.ForEach(i => { i.ItemStatus = "Ordered"; i.Product.StockQuantity -= i.Quantity; i.OrderId = NewOrder.Id; });
        
        if (ItemStatus.Any(i => i.Product.StockQuantity == 0))
        {
            ItemStatus.ForEach(i => {i.Product.IsAvailable = "OutOfStock"; });
        }
        await _context.SaveChangesAsync();

        return Ok("Order Added successfully");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
