using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }


    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // cashier seed data
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier { Id = 1, FirstName = "Will", LastName = "Johnson" },
            new Cashier { Id = 2, FirstName = "Chloe", LastName = "Smith" },
            new Cashier { Id = 3, FirstName = "Mike", LastName = "Anderson" },
            new Cashier { Id = 4, FirstName = "Hillary", LastName = "West" },

        });

        // product seed data
        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product { Id = 1, ProductName = "Doritos", Brand = "Frito-Lay", CategoryId = 1, Price = 2.99M },
            new Product { Id = 2, ProductName = "Chex Mix", Brand = "General Mills", CategoryId = 1, Price = 3.99M },
            new Product { Id = 3, ProductName = "Cheetos", Brand = "Frito-Lay", CategoryId = 1, Price = 2.99M },
            new Product { Id = 4, ProductName = "Coke", Brand = "Coca-Cola", CategoryId = 2, Price = 3.99M },
            new Product { Id = 5, ProductName = "Diet Coke", Brand = "Coca-Cola", CategoryId = 2, Price = 3.99M },
            new Product { Id = 6, ProductName = "Monster Energy", Brand = "Monster Beverage Corporation", CategoryId = 2, Price = 2.99M },
            new Product { Id = 7, ProductName = "Digiorno Pizza", Brand = "Nestle Global", CategoryId = 3, Price = 8.99M },
            new Product { Id = 8, ProductName = "Ham & American Cheese Sandwich", Brand = "Twice Daily", CategoryId = 3, Price = 5.99M },
            new Product { Id = 9, ProductName = "Paper Towels", Brand = "Downy", CategoryId = 4, Price = 4.99M },
            new Product { Id = 10, ProductName = "Batteries", Brand = "Duracell", CategoryId = 4, Price = 1.99M },
        });

        // category seed data
        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category { Id = 1, CategoryName = "Snack" },
            new Category { Id = 2, CategoryName = "Drink" },
            new Category { Id = 3, CategoryName = "Meal" },
            new Category { Id = 4, CategoryName = "Household" }

        });

        // order seed data
        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order { Id = 1, CashierId = 2, PaidOnDate = new DateTime(2024, 01, 12, 08, 00, 00) },
            new Order { Id = 2, CashierId = 1, PaidOnDate = new DateTime(2024, 01, 13, 09, 00, 00) },
            new Order { Id = 3, CashierId = 4, PaidOnDate = new DateTime(2024, 01, 13, 10, 00, 00) },
            new Order { Id = 4, CashierId = 3, PaidOnDate = new DateTime(2024, 01, 12, 10, 00, 00) },
        });

        // order product seed data
        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
           {
            new OrderProduct { Id = 1, ProductId = 1, OrderId = 2, Quantity = 2},
            new OrderProduct { Id = 2, ProductId = 1, OrderId = 1, Quantity = 1},
            new OrderProduct { Id = 3, ProductId = 2, OrderId = 3, Quantity = 3},
            new OrderProduct { Id = 4, ProductId = 3, OrderId = 2, Quantity = 4},
            new OrderProduct { Id = 5, ProductId = 4, OrderId = 4, Quantity = 2},
            new OrderProduct { Id = 6, ProductId = 3, OrderId = 2, Quantity = 7},
            new OrderProduct { Id = 7, ProductId = 4, OrderId = 2, Quantity = 2},
           });

    }
}