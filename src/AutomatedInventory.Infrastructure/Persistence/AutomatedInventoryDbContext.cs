using AutomatedInventory.Domain.Entities;
using AutomatedInventory.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Persistence;

public sealed class AutomatedInventoryDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AutomatedInventoryDbContext(DbContextOptions<AutomatedInventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SalesOrder> Orders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> OrderLines => Set<SalesOrderLine>();
    public DbSet<InventoryTransaction> Transactions => Set<InventoryTransaction>();
    public DbSet<SupplyRecord> SupplyRecords => Set<SupplyRecord>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasIndex(product => product.Sku)
            .IsUnique();

        modelBuilder.Entity<SalesOrderLine>()
            .Property(line => line.ProductName)
            .HasMaxLength(200);

        modelBuilder.Entity<SalesOrder>()
            .HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey(line => line.SalesOrderId);

        modelBuilder.Entity<SalesOrder>()
            .Navigation(order => order.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.Entity<Customer>()
            .HasIndex(customer => customer.Email)
            .IsUnique();

        modelBuilder.Entity<Supplier>()
            .HasIndex(supplier => supplier.Email)
            .IsUnique();

        modelBuilder.Entity<UserActivity>()
            .HasIndex(activity => new { activity.UserId, activity.OccurredAtUtc });
    }
}
