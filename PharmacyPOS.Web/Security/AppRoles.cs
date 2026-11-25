namespace PharmacyPOS.Web.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";

    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[] { Admin, Manager, Cashier });
    public static IReadOnlyList<string> Staff { get; } = Array.AsReadOnly(new[] { Manager, Cashier });
}

public static class AccessPolicies
{
    public const string ProductRead = "ProductRead";
    public const string ProductWrite = "ProductWrite";
}
