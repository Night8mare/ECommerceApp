using System;

namespace ECommerceApp.DTO
{
    public class RegisterDTO
    {

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNo { get; set; }
        public required string Country { get; set; }
        public required string State { get; set; }
        public required string City { get; set; }
        public required string Address { get; set; }
        public required string PostalCard { get; set; }
        public required string PasswordHash { get; set; }
    }
}