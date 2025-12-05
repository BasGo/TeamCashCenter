using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class UserService(UserManager<User> userManager, RoleManager<Role> roleManager) : IUserService
{
    public async Task<IList<User>> GetUsersInRoleAsync(string roleName)
    {
        return await userManager.GetUsersInRoleAsync(roleName);
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Role?> GetRoleById(Guid roleId)
    {
        return await roleManager.FindByIdAsync(roleId.ToString());
    }
}