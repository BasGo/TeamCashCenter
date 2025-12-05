using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Services;

namespace TeamCashCenter.Data;

public static class DbSeeder
{
    // admin role
    private const string adminRoleName = "Admin";
    private const string adminRoleDescription = "Administratoren (nicht löschbar)";
    
    public static async Task SeedAsync(CashCenterContext db)
    {
        if (!await db.TransactionTypes.AnyAsync())
        {
            db.TransactionTypes.AddRange( new List<TransactionType> {
                new() { Name = "Umbuchung", IsIncome = true, IsRegularFee = false},
                new() { Name = "Einzahlung", IsIncome = true, IsRegularFee = false},
                new() { Name = "Mitgliedsbeitrag", IsIncome = true, IsRegularFee = true},
                new() { Name = "Spende", IsIncome = true, IsRegularFee = false},
                new() { Name = "Sonstiges", IsIncome = false }
            });
        }

        if (!await db.Accounts.AnyAsync())
        {
            db.Accounts.Add(new Account("Vereinskonto", "Hauptkonto", "DE00 0000 0000 0000", 0m, 1));
            db.Accounts.Add(new Account("Schiedsrichterkasse", "Auslagen für den Schiedsrichter", 0m, 2));
            db.Accounts.Add(new Account("Sonstiges", "Sonstige Auslagen", 0m));
        }

        await db.SaveChangesAsync();
    }

    public static async Task SeedAdminAsync(this IServiceScope scope, AppOptions appOptions)
    {
        //using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        // ensure the 'Admin' role exists and assign the user to it
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new Role(adminRoleName, adminRoleDescription));
        }
        
        // try to find existing admin user: if not existent, create a new one
        var adminEmail = appOptions.AdminEmail ?? "admin.verein.local";
        var adminPassword = appOptions.AdminPassword ?? "Admin123!";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new User { UserName = "Administrator", Email = adminEmail, EmailConfirmed = true, FirstName = "Local", LastName = "Admin" };
            await userManager.CreateAsync(admin, adminPassword);
        }

        // ensure that the admin user is in the role 'Admin'
        if (!await userManager.IsInRoleAsync(admin, adminRoleName))
        {
            await userManager.AddToRoleAsync(admin, adminRoleName);
        }
    }
}
