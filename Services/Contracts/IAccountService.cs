using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services.Contracts;

public interface IAccountService
{
    public Task<Account?> GetByIdAsync(Guid? accountId);
    public Task UpsertAccountAsync(Account account, bool saveImmediately = true);
    public Task CreateAccountAsync(Account account, bool saveImmediately = true);
    public Task UpdateAccountAsync(Account account, bool saveImmediately = true);
    public Task<List<Account>> GetAccountsAsync();
    public Task<Account?> GetAccountAsync(Guid accountId);
    public Task<bool> UpdateAmount(Guid accountId, decimal amount, bool saveImmediately = true);
    public Task<bool> RecalculateAsync(Guid accountId);
}