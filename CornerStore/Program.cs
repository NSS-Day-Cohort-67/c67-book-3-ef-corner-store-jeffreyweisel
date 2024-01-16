using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Cashier endpoints --------------------------------------------


// Get a cashier (include their orders, and the orders' products).
app.MapGet("/api/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    return db.Cashiers
       .Include(c => c.Orders)
        .ThenInclude(o => o.OrderProducts)
        .ThenInclude(op => op.Product)  
        .Select(c => new CashierDTO
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Orders = c.Orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                PaidOnDate = o.PaidOnDate,
                OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
                {
                    Id = op.Id,
                    ProductId = op.ProductId,
                    Product = new ProductDTO
                    {
                        Id = op.Product.Id,
                        ProductName = op.Product.ProductName,
                        Price = op.Product.Price,
                        Brand = op.Product.Brand,
                        CategoryId = op.Product.CategoryId,
                        Category = new CategoryDTO
                        {
                            Id = op.Product.Category.Id,
                            CategoryName = op.Product.Category.CategoryName
                        }

                    },
                    Quantity = op.Quantity

                }).ToList()
            }).ToList()
        })
        .FirstOrDefault(c => c.Id == id);
});


// Add a cashier

// {
//     "firstName": "Mark",
//     "lastName": "Stewart"
// }

app.MapPost("/api/cashiers", (CornerStoreDbContext db, Cashier newCashier) =>
{
    try
    {
        db.Cashiers.Add(newCashier);
        db.SaveChanges();
        return Results.Created($"/cashiers/{newCashier.Id}", newCashier);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});


// Product endpoints ---------------------------------------------

// Get all products with categories. If the search query string param is present, return only products whose names or category names include the search value (ignore case).

// https://localhost:7065/api/products?search=household

app.MapGet("/api/products", (CornerStoreDbContext db, string? search) =>
{
    IQueryable<ProductDTO> productQuery = db.Products   //
        .Include(p => p.Category)
        .Select(p => new ProductDTO
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Price = p.Price,
            Brand = p.Brand,
            CategoryId = p.CategoryId,
            Category = new CategoryDTO
            {
                Id = p.Category.Id,
                CategoryName = p.Category.CategoryName
            }
        });

        if(!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            productQuery = productQuery.Where(p =>
            p.ProductName.ToLower().Contains(search) || p.Category.CategoryName.ToLower().Contains(search));
        }

        return productQuery.ToList();
        
});

// Add a product

// {
//     "productName": "Cheerios",
//     "price": 4.99,
//     "brand": "General Mills",
//     "categoryId": 3
// }

app.MapPost("/api/products", (CornerStoreDbContext db, Product newProduct) =>
{
    try
    {
        db.Products.Add(newProduct);
        db.SaveChanges();
        return Results.Created($"/products/{newProduct.Id}", newProduct);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

// Update a product

// https://localhost:7065/api/products/13

app.MapPut("/api/products/{id}", (CornerStoreDbContext db, int id, Product product) =>
{
    Product productToUpdate = db.Products.SingleOrDefault(product => product.Id == id);
    if (productToUpdate == null)
    {
        return Results.NotFound();
    }

    productToUpdate.ProductName = product.ProductName;
    productToUpdate.Price = product.Price;
    productToUpdate.Brand = product.Brand;
    productToUpdate.CategoryId = product.CategoryId;


    db.SaveChanges();
    return Results.Ok(productToUpdate);
});

// Order endpoints ------------------------------------------------

// Get all orders. Check for a query string param orderDate that only returns orders from a particular day. If it is not present, return all orders.

// https://localhost:7065/api/orders?orderDate=2024-01-12T08:00:00

app.MapGet("/api/orders", (CornerStoreDbContext db, DateTime? orderDate) =>
{
   IQueryable<OrderDTO> orderQuery = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .ThenInclude(p => p.Category)
        .Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            Cashier = new CashierDTO
            {
                Id = o.Cashier.Id,
                FirstName = o.Cashier.FirstName,
                LastName = o.Cashier.LastName
            },
            PaidOnDate = o.PaidOnDate,
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                Id = op.Id,
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                },
                Quantity = op.Quantity
            }).ToList()
        });

        if(orderDate.HasValue)
        {
            orderQuery = orderQuery.Where(o => o.PaidOnDate == orderDate);
        }

        return orderQuery.ToList();
});

// Get an order details, including the cashier, order products, and products on the order with their category.
app.MapGet("/api/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    return db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .ThenInclude(p => p.Category)
        .Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            Cashier = new CashierDTO
            {
                Id = o.Cashier.Id,
                FirstName = o.Cashier.FirstName,
                LastName = o.Cashier.LastName
            },
            PaidOnDate = o.PaidOnDate,
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                Id = op.Id,
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                },
                Quantity = op.Quantity
            }).ToList()
        })
        .FirstOrDefault(c => c.Id == id);
});



// Delete an order
app.MapDelete("/api/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order orderToDelete = db.Orders.SingleOrDefault(o => o.Id == id);
    if (orderToDelete == null)
    {
        return Results.NotFound();
    }
    db.Orders.Remove(orderToDelete);
    db.SaveChanges();
    return Results.Ok(orderToDelete);

});


// Create a new order (with products)

// {
//     "cashierId": 3,
//     "paidOnDate": "2024-01-16T08:00:00",
//     "orderProducts": [
//     {
//       "productId": 7,
//       "quantity": 2
//     },
//     {
//       "productId": 2,
//       "quantity": 1
//     }
//   ]
// }
app.MapPost("/api/orders", (CornerStoreDbContext db, Order newOrder) =>
{

    try
    {

        db.Orders.Add(newOrder);
        db.SaveChanges();
        return Results.Created($"/orders/{newOrder.Id}", newOrder);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});


app.Run();

//don't move or change this!
public partial class Program { }