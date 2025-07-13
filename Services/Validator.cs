using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ECommerceApp.DTO;

namespace ECommerceApp.Services
{
    public static class Validator
    {
        public static void ValidateRegistry([FromBody] RegisterDTO user, ModelStateDictionary ModelState, ApplicationDbContext _context)
        {
            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrWhiteSpace(user.FirstName))
            {
                ModelState.AddModelError("FirstName", "Please provide your First Name.");
            }

            if (string.IsNullOrEmpty(user.LastName) || string.IsNullOrWhiteSpace(user.LastName))
            {
                ModelState.AddModelError("LastName", "Please provide your Last Name.");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError("Email", "Please provide your Email.");
            }
            else if (!user.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError("Email", "Only Gmail addresses are allowed.");
            }
            else if (user.Email.Count() < 20)
            {
                ModelState.AddModelError("Email", "Your email is too short!");
            }
            else if (_context.Users.Any(c => c.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered");
            }

            if (string.IsNullOrEmpty(user.PhoneNo) || string.IsNullOrWhiteSpace(user.PhoneNo))
            {
                ModelState.AddModelError("PhoneNo", "Please provide your phone number");
            }
            else if (!user.PhoneNo.StartsWith("+"))
            {
                ModelState.AddModelError("PhoneNo", "Please provide your international dialing country code before your phone number");
            }

            if (string.IsNullOrEmpty(user.Country) || string.IsNullOrWhiteSpace(user.Country))
            {
                ModelState.AddModelError("Country", "Please provide your Country");
            }

            if (string.IsNullOrEmpty(user.State) || string.IsNullOrWhiteSpace(user.State))
            {
                ModelState.AddModelError("State", "Please provide your State");
            }

            if (string.IsNullOrEmpty(user.City) || string.IsNullOrWhiteSpace(user.City))
            {
                ModelState.AddModelError("City", "Please provide your City");
            }

            if (string.IsNullOrEmpty(user.Address) || string.IsNullOrWhiteSpace(user.Address))
            {
                ModelState.AddModelError("Address", "Please provide your Address");
            }

            if (string.IsNullOrEmpty(user.PostalCard) || string.IsNullOrWhiteSpace(user.PostalCard))
            {
                ModelState.AddModelError("PostalCard", "Please provide your Postal Card");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError("Password", "Please provide your password");
            }
            else if (user.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("Password", "Your password should be longer than 6 characters");
            }
        }
        public static void ValidateAdminRegistry([FromBody]AdminDTO user, ModelStateDictionary ModelState, ApplicationDbContext _context)
        {
            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrWhiteSpace(user.FirstName))
            {
                ModelState.AddModelError("FirstName", "Please provide your First Name.");
            }

            if (string.IsNullOrEmpty(user.LastName) || string.IsNullOrWhiteSpace(user.LastName))
            {
                ModelState.AddModelError("LastName", "Please provide your Last Name.");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError("Email", "Please provide your Email.");
            }
            else if (!user.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError("Email", "Only Gmail addresses are allowed.");
            }
            else if (user.Email.Count() < 20)
            {
                ModelState.AddModelError("Email", "Your email is too short!");
            }
            else if (_context.Users.Any(c => c.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered");
            }

            if (string.IsNullOrEmpty(user.PhoneNo) || string.IsNullOrWhiteSpace(user.PhoneNo))
            {
                ModelState.AddModelError("PhoneNo", "Please provide your phone number");
            }
            else if (!user.PhoneNo.StartsWith("+"))
            {
                ModelState.AddModelError("PhoneNo", "Please provide your international dialing country code before your phone number");
            }

            if (string.IsNullOrEmpty(user.Country) || string.IsNullOrWhiteSpace(user.Country))
            {
                ModelState.AddModelError("Country", "Please provide your Country");
            }

            if (string.IsNullOrEmpty(user.State) || string.IsNullOrWhiteSpace(user.State))
            {
                ModelState.AddModelError("State", "Please provide your State");
            }

            if (string.IsNullOrEmpty(user.City) || string.IsNullOrWhiteSpace(user.City))
            {
                ModelState.AddModelError("City", "Please provide your City");
            }

            if (string.IsNullOrEmpty(user.Address) || string.IsNullOrWhiteSpace(user.Address))
            {
                ModelState.AddModelError("Address", "Please provide your Address");
            }

            if (string.IsNullOrEmpty(user.PostalCard) || string.IsNullOrWhiteSpace(user.PostalCard))
            {
                ModelState.AddModelError("PostalCard", "Please provide your Postal Card");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError("Password", "Please provide your password");
            }
            else if (user.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("Password", "Your password should be longer than 6 characters");
            }
        }
    }
}