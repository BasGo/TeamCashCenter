using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Data.Extensions;

public static class AccountExtensions
{
    public static void IncreaseBalance(this CashCenterContext Db, Account? account, decimal? amount)
    {
        if (account == null || !amount.HasValue) return;
        account.Balance += amount.Value;
        Db.Accounts.Update(account);
    }
    
    public static void DecreaseBalance(this CashCenterContext Db, Account? account, decimal? amount)
    {
        if (account == null || !amount.HasValue) return;
        account.Balance -= amount.Value;
        Db.Accounts.Update(account);
    }
}