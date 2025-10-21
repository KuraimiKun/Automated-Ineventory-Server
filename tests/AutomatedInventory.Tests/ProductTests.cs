using System;
using AutomatedInventory.Domain.Entities;
using Xunit;

namespace AutomatedInventory.Tests;

public sealed class ProductTests
{
    [Fact]
    public void IncreaseStock_WhenQuantityPositive_AddsToStock()
    {
        var product = new Product
        {
            Name = "Servo Motor",
            Sku = "SRV-001",
            Description = "High torque servo motor",
            UnitPrice = 49.99m
        };

        product.InitializeStock(10);
        product.IncreaseStock(5);

        Assert.Equal(15, product.StockQuantity);
    }

    [Fact]
    public void DecreaseStock_WhenQuantityExceeds_Throws()
    {
        var product = new Product
        {
            Name = "Servo Motor",
            Sku = "SRV-001",
            Description = "High torque servo motor",
            UnitPrice = 49.99m
        };

        product.InitializeStock(3);

        Assert.Throws<InvalidOperationException>(() => product.DecreaseStock(5));
    }
}
