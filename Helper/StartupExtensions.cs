using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Helper;

public static class StartupExtensions
{
    public static void Configure<T>(this WebApplicationBuilder builder, string configSectionName) where T : class, new()
    {
        builder.Services.Configure<T>(builder.Configuration.GetSection(configSectionName));
    }

    public static async Task MigrateDatabaseAsync(this IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<CashCenterContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task SeedDataAsync(this IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<CashCenterContext>();
        await DbSeeder.SeedAsync(db);
    }
    
    
    
    public static async Task CreateRoles(this IServiceScope scope)
    {
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            var roles = new Dictionary<string, string>
            {
                { "Admin", "Admin" },
                { "Manager", "Default role for managers" },
                { "Schatzmeister", "Default role for treasurer" },
                { "Trainer", "Default role for trainer" },
                { "Spieler", "Default role for players" }
            };
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Key))
                {
                    await roleManager.CreateAsync(new Role(role.Key, role.Value));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Role creation failed: " + ex.Message);
        }
    }
}