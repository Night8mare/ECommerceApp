using System;

namespace ECommerceApp.DTO
{
    public class LoginDTO
    {
        
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}