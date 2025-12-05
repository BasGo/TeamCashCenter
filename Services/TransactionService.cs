using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Helper;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class TransactionService(ILogger<TransactionService> logger, IAccountService accountService, CashCenterContext Db) : ITransactionService
{
    public async Task<Transaction?> GetAsync(Guid referenceId, Guid accountId)
    {
        return await Db.Transactions.FirstOrDefaultAsync(t => t.Reference == referenceId && t.AccountId == accountId);
    }

    public async Task<Guid?> Revoke(Guid transactionId, bool saveImmediately = true)
    {
        var old = await Db.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);
        if (old == null) return null;
        var revertedTransaction = old.GetCopy();
        revertedTransaction.Id = Guid.Empty;
        revertedTransaction.Description = $"[Korrektur] {revertedTransaction.Description}";
        revertedTransaction.Amount = old.Amount * -1;
        revertedTransaction.ParentId = old.Id;

        return await Insert(revertedTransaction, saveImmediately);
    }

    public async Task<Guid?> Revoke(Transaction? transaction, bool saveImmediately = true)
    {
        if (transaction == null) return null;
        return await Revoke(transaction.Id, saveImmediately);
    }

    public async Task<Guid> Insert(Transaction transaction, bool saveImmediately = true)
    {
        transaction.Date = DateTime.Now;
        Db.Transactions.Add(transaction);
        await Db.SaveChangesAsyncIf(saveImmediately);
        
        await accountService.UpdateAmount(transaction.AccountId, transaction.Amount, saveImmediately);
        await Db.SaveChangesAsyncIf(saveImmediately);
        var acc = await accountService.GetAccountAsync(transaction.AccountId) ?? new Account();
        
        logger.LogInformation("Inserted transaction: {TransactionBookingDate} -> {AccName} -> {TransactionAmount} -> {TransactionDescription}", transaction.BookingDate, acc.Name, transaction.Amount, transaction.Description);
        return transaction.Id;
    }
}