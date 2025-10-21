using AutomatedInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutomatedInventory.Infrastructure.Persistence;

public sealed class AutomatedInventoryDbContext : DbContext
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
    }
}
