using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using ECommerceApp.DTO;
using ECommerceApp.Services;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;


namespace ECommerceApp.Controllers
{
    public class AccountServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public AccountServiceController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("api/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("Invalid request body. Expected JSON object.");
                }

                Validator.ValidateRegistry(user, ModelState, _context);

                if (ModelState.IsValid)
                {
                    var NewUser = new User
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
                    var passwordHasher = new PasswordHasher<User>().HashPassword(NewUser, user.PasswordHash);
                    NewUser.PasswordHash = passwordHasher;
                    await _context.Users.AddAsync(NewUser);
                    await _context.SaveChangesAsync();

                    var Cart = await _context.Carts.AnyAsync(c => c.UserId == NewUser.Id);
                    if (!Cart)
                    {
                        var NewCart = new Cart { UserId = NewUser.Id };
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
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
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
                var passwordHasher = new PasswordHasher<User>();

                var user = await _context.Users.FirstOrDefaultAsync(c => c.Email == login.Email);

                if (user == null)
                {
                    return BadRequest("Invalid username or password!");
                }
                if (login.Email != user.Email)
                {
                    return BadRequest("Invalid username or password!");
                }

                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, login.PasswordHash);

                if (passwordVerificationResult != PasswordVerificationResult.Success)
                {
                    return BadRequest("Invalid username or password!");
                }

                var Cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (Cart == null)
                {
                    var NewCart = new Cart { UserId = user.Id };
                    await _context.Carts.AddAsync(NewCart);
                    var accessToken = CreateToken(user);
                    user.Token = accessToken;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return Ok(accessToken);
                }
                var token = CreateToken(user);
                user.Token = token;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return Ok(token);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal Server Error: {e.Message}");
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("userId", user.Id.ToString())

            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: user.Role == "Admin"
                        ? DateTime.UtcNow.AddDays(7)
                        : DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}