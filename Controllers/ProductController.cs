using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceApp.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "User")]
    [HttpGet("api/Product")]
    public async Task<IActionResult> Product()
    {
        var list = await _context.Products.Where(p => p.IsAvailable == "InStock" && p.StockQuantity != 0)
                            .Select(p => new {
                                ProductName = p.Name,
                                ProductDiscreption = p.Description,
                                ProductPrice = p.PurchasePrice,
                                Stock = p.StockQuantity,
                                Available = p.IsAvailable}).ToListAsync();
        return Ok(list);
    }
    
    [Authorize(Roles = "User")]
    [HttpGet("api/Product/Filter")]
    public async Task<IActionResult> ProductFilter(decimal? minPrice, decimal? maxPrice, string? AscOrder, string? search)
    {
        var query = _context.Products.Where(p => p.IsAvailable == "InStock" && p.StockQuantity != 0).Select(p =>
                                                new {
                                                    ProductName = p.Name,
                                                    ProductDiscreption = p.Description,
                                                    ProductPrice = p.PurchasePrice, Stock =
                                                    p.StockQuantity,
                                                    Available = p.IsAvailable }).AsQueryable();

        if (minPrice.HasValue)
        {
            query = query.Where(q => q.ProductPrice >= minPrice);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(q => q.ProductPrice <= maxPrice);
        }

        if (!string.IsNullOrEmpty(AscOrder) && AscOrder.ToLower() == "asc")
        {
            query = query.OrderBy(p => p.ProductPrice);
        }

        if (!string.IsNullOrEmpty(AscOrder) && AscOrder.ToLower() == "desc")
        {
            query = query.OrderByDescending(p => p.ProductPrice);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.ProductName.Contains(search));
        }

        var result = await query.ToListAsync();
        if (result == null || result.Count == 0)
        {
            return BadRequest("There is no item available");
        }
        return Ok(result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
