using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using ECommerceApp.Services;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Identity;


namespace ECommerceApp.Controllers;

public class AccountServiceController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountServiceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("api/register")]
    public async Task<IActionResult> Register([FromBody] Customer user)
    {
        try
        {
            Validator.ValidateRegistry(user, ModelState, _context);

            if (ModelState.IsValid)
            {
                var NewUser = new Customer
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNo,
                    Country = user.Country,
                    State = user.State,
                    City = user.City,
                    Address = user.Address,
                    PostalCard = user.PostalCard,
                };
                var passwordHasher = new PasswordHasher<Customer>();
                NewUser.PasswordHash = passwordHasher.HashPassword(NewUser, user.PasswordHash);
                await _context.Customers.AddAsync(NewUser);
                await _context.SaveChangesAsync();

                var Cart = await _context.Carts.AnyAsync(c => c.CustomerId == NewUser.Id);
                if (!Cart)
                {
                    var NewCart = new Cart { CustomerId = NewUser.Id };
                    await _context.Carts.AddAsync(NewCart);
                    await _context.SaveChangesAsync();
                    return Ok("User Registered Successfully");
                }
                return Ok("User registered successfully");
            }
            return BadRequest(ModelState);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Internal Server Error: {e.Message}");
        }
    }

    [HttpPost("api/Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        try
        {
            
            if (login == null || string.IsNullOrEmpty(login.Email))
            {
                return BadRequest("Please provide your Email");
            }

            if (string.IsNullOrEmpty(login.PasswordHash))
            {
                return BadRequest("Please provide your password");
            }
            var passwordHasher = new PasswordHasher<Customer>();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == login.Email);

            if (customer == null)
            {
                return BadRequest("Invalid username or password!");
            }
            if (login.Email != customer.Email)
            {
                return BadRequest("Invalid username or password!");
            }

            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, login.PasswordHash);

            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return BadRequest("Invalid username or password!");
            }

            var Cart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customer.Id);
            if (Cart == null)
            {
                var NewCart = new Cart { CustomerId = customer.Id };
                await _context.Carts.AddAsync(NewCart);
                await _context.SaveChangesAsync();
                return Ok("Logged in Successfully");
            }
            return Ok("Logged in successfully");
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Internal Server Error: {e.Message}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
