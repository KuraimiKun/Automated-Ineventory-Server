namespace AutomatedInventory.Application.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string InventoryManager = "InventoryManager";
    public const string Cashier = "Cashier";

    public static readonly string[] All = { Admin, InventoryManager, Cashier };
}
