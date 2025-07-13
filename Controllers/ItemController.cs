using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceApp.Controllers;

public class ItemController : Controller
{
    private readonly ApplicationDbContext _context;

    public ItemController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "User")]
    [HttpGet("api/ItemView")]
    public async Task<IActionResult> ItemView(int? CartId)
    {
        if (CartId.HasValue)
        {
            var CartExist = await _context.Carts.FindAsync(CartId);

            if (CartExist == null)
            {
                return BadRequest("Cart not found");
            }

            var ItemList = await _context.Items.Where(c => c.CartId == CartId && c.ItemStatus == "Pending" && c.OrderId == null)
                                        .OrderBy(i => i.Id)
                                        .Select(c => new
                                        {
                                            ProductName = c.Product.Name,
                                            Quantity = c.Quantity,
                                            ProductPrice = c.Product.PurchasePrice,
                                            TotalItemPrice = c.TotalAmount,
                                            Status = c.ItemStatus
                                        }).ToListAsync();

            if (!ItemList.Any())
            {
                return Ok("No items in the cart");
            }

            var totalPrice = ItemList.Sum(c => c.TotalItemPrice);

            return Ok(new { ItemItems = ItemList, TotalPrice = totalPrice });
        }
        return NotFound("Please provide cart ID");
    }

    [Authorize(Roles = "User")]
    [HttpPost("api/AddItem")]
    public async Task<IActionResult> AddItem(int? CartId, int? productId, int? quantity)
    {
        if (!CartId.HasValue)
        {
            return NotFound("You need to provide cart ID");
        }
        else if (!productId.HasValue)
        {
            return NotFound("You Need to provide product ID");
        }
        else if (!quantity.HasValue)
        {
            return NotFound("You need to provide the quantity for the item choosen");
        }

        var CartExist = await _context.Carts.FindAsync(CartId);
        var ProductExist = await _context.Products.FindAsync(productId);

        if (CartExist == null)
        {
            return NotFound("No matching Cart");
        }
        else if (ProductExist == null)
        {
            return NotFound("No matching Item");
        }
        else if (ProductExist.IsAvailable == "OutOfStock" || ProductExist.StockQuantity == 0)
        {
            return BadRequest("This item is currently out of stock");
        }

        var ItemExcist = await _context.Items.FirstOrDefaultAsync(c => c.ProductId == productId && c.CartId == CartId && c.OrderId == null);

        if (ItemExcist != null)
        {
            return BadRequest($"Item already exist, to update {ItemExcist.Product.Name} quantity use the update method");
        }

        if (quantity > ProductExist.StockQuantity)
        {
            return BadRequest($"The avaliable Stock {ProductExist.StockQuantity}\nPlease edit your desired quantity");
        }

        var AddToItem = new Item { CartId = CartExist.Id, ProductId = ProductExist.Id, Quantity = quantity.Value, TotalAmount = quantity.Value * ProductExist.PurchasePrice, ItemStatus = "Pending" };
        var Item = await _context.Items.AddAsync(AddToItem);
        await _context.SaveChangesAsync();
        return Ok("Added successfully");
    }

    [Authorize(Roles = "User")]
    [HttpPut("api/UpdateQuantity")]
    public async Task<IActionResult> UpdateQuantity(int? cartId, int? productId, int? quantity)
    {
        if (!cartId.HasValue)
        {
            return NotFound("You need to provide cart ID");
        }
        else if (!productId.HasValue)
        {
            return NotFound("You Need to provide product ID");
        }
        else if (!quantity.HasValue)
        {
            return NotFound("You need to provide the quantity for the item choosen");
        }

        var ItemExcist = await _context.Items.Include(i => i.Product).FirstOrDefaultAsync(i => i.CartId == cartId && i.Product.Id == productId && i.ItemStatus == "Pending");

        if (ItemExcist == null)
        {
            return NotFound("Item not found");
        }

        if (ItemExcist.Product.IsAvailable == "OutOfStock")
        {
            return BadRequest("This item is out of stock right now");
        }

        if (ItemExcist.OrderId != null)
        {
            return BadRequest("You can`t change an ordered item quantity");
        }

        if (quantity > ItemExcist.Product.StockQuantity)
        {
            return BadRequest($"The remaining stock avaliable {ItemExcist.Product.StockQuantity}");
        }

        ItemExcist.Quantity = quantity.Value;
        ItemExcist.TotalAmount = quantity.Value * ItemExcist.Product.PurchasePrice;
        await _context.SaveChangesAsync();
        return Ok("Updated Quantity");
    }

    [Authorize(Roles = "User")]
    [HttpDelete("api/RemoveItem")]
    public async Task<IActionResult> RemoveItem(int? CartId, int? productId)
    {
        if (!CartId.HasValue)
        {
            return BadRequest("Please provide Cart ID");
        }
        else if (!productId.HasValue)
        {
            return BadRequest("Please provide product ID");
        }

        var CartExist = await _context.Carts.FindAsync(CartId);
        var ProductExist = await _context.Products.FindAsync(productId);
        if (CartExist == null)
        {
            return Ok("No matching Cart");
        }
        else if (ProductExist == null)
        {
            return Ok("No matching Item");
        }

        var ItemExcist = await _context.Items.FirstOrDefaultAsync(i => i.ProductId == productId && i.CartId == CartId && i.ItemStatus == "Pending" && i.OrderId == null);
        if (ItemExcist != null)
        {
            _context.Items.Remove(ItemExcist);
            await _context.SaveChangesAsync();
            return Ok("Removed successfully");
        }
        return Ok("No matching Item in your cart");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
