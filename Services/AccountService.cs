using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Helper;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class AccountService(ILogger<AccountService> logger, CashCenterContext Db) : IAccountService
{
    public async Task<Account?> GetAccountAsync(Guid accountId)
    {
        return await Db.Accounts.FindAsync(accountId);
    }

    public async Task<bool> UpdateAmount(Guid accountId, decimal amount, bool saveImmediately = true)
    {
        var account = await Db.Accounts.FindAsync(accountId);
        if (account == null)
            return false;
        
        account.Balance += amount;

        await Db.SaveChangesAsyncIf(saveImmediately);
        
        logger.LogInformation("Updating account balance for account '{accountName}' -> {AccountBalance} ({Amount})", account.Name, account.Balance, amount);
        return true;
    }

    public async Task<bool> RecalculateAsync(Guid accountId)
    {
        var account = await Db.Accounts.Include(t => t.Transactions).FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return false;
        
        var amount = account.StartBalance + account.Transactions.Sum(t => t.Amount);
        account.Balance = amount;
        await Db.SaveChangesAsync();
        return true;
    }

    public async Task CreateAccountAsync(Account account, bool saveImmediately = true)
    {
        if (account.Id != Guid.Empty) return;
        Db.Accounts.Add(account);
        await Db.SaveChangesAsyncIf(saveImmediately);
    }
    
    public async Task UpdateAccountAsync(Account account, bool saveImmediately = true)
    {
        if (account.Id == Guid.Empty) return;
        Db.Accounts.Update(account);
        await RecalculateAsync(account.Id);
        await Db.SaveChangesAsyncIf(saveImmediately);
    }
    
    public async Task UpsertAccountAsync(Account account, bool saveImmediately = true)
    {
        if (account.Id == Guid.Empty)
        {
            await CreateAccountAsync(account, saveImmediately);
            return;
        }
        
        await UpdateAccountAsync(account, saveImmediately);
    }

    public async Task<Account?> GetByIdAsync(Guid? accountId)
    {
        if (!accountId.HasValue) return null;
        return await Db.Accounts.FindAsync(accountId.Value);
    }
    
    public async Task<List<Account>> GetAccountsAsync()
    {
        return await Db.Accounts.Include(t => t.Transactions).OrderBy(a => a.Order).ToListAsync();
    }
}