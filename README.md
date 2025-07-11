# 🛒 E-Commerce API (.NET Core)

This is a simple back-end E-Commerce RESTful API built with ASP.NET Core and Entity Framework Core, connected to a SQL Server database. It includes user registration/login, cart management, product stock handling, and order placement.

---

## 📦 Features

- ✅ User registration and login with password hashing
- 🛒 Cart management (add, update, delete items)
- 📦 Product quantity tracking
- 📃 Place order and deduct from stock
- 🔐 Simple validation and error handling
- 📁 SQL `.bak` file provided for database

---

## ⚙️ Installation

### 1. Clone the repository

```bash
git clone https://github.com/Night8mare/ECommerceApp
cd ECommerceApp

2. Restore the Database

Open SQL Server Management Studio (SSMS).

Right-click on Databases → Restore Database.

Select Device → Browse for the provided .bak file (ECommerce.bak).

Restore with name: ECommerce (or match your config).

Click OK.

run the Product.sql to update the Product list

3. Update the connection string

In appsettings.json, configure the connection:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ECommerce;Trusted_Connection=True;TrustServerCertificate=True;"
}

Replace YOUR_SERVER_NAME with your local SQL Server instance name.

4. Scaffold the database
dotnet ef dbcontext scaffold "Server=YOUR_SERVER_NAME;Database=ECommerce;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models --context ApplicationDbContext --use-database-names

5. Run the project

dotnet run

The API will be available at:
http://localhost:PORT/api/
http://localhost:PORT/swagger

Or you can use 
API Endpoints

*POST api/register
Body (JSON)
{
    "FirstName": "",
    "LastName": "",
    "Email": "",
    "PhoneNo": "",
    "Country": "",
    "State": "",
    "City": "",
    "Address": "",
    "PostalCard": "",
    "PasswordHash": ""
}
*POST api/Login
Body (JSON)
{
    "Email": "",
    "PasswordHash": ""
}
*GET api/Product
*GET api/Product/Filter
Query Parameters:
?minPrice=50&maxPrice=60&AscOrder=asc&search=stand
*POST api/AddItem
Query Parameters:
?CartId=6&productId=1&quantity=5
*GET api/ItemView
Query Parameters:
?CartId=6
*PUT api/UpdateQuantity
Query Parameters:
?cartId=6&productId=1&quantity=7
*DELETE api/RemoveItem
Query Parameters:
?CartId=6&productId=1
*POST api/AddOrder
Query Parameters:
?CartNo=6
*POST api/ItemHistory
Query Parameters:
?OrderId=9


