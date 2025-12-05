using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services.Contracts;

public interface IUserService
{
    public Task<IList<User>> GetUsersInRoleAsync(string roleName);

    public Task<List<Role>> GetRolesAsync();

    public Task<Role?> GetRoleById(Guid roleId);
}