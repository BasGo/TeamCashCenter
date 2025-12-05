using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services.Contracts;

public interface ITransactionService
{
    public Task<Transaction?> GetAsync(Guid referenceId, Guid accountId);
    public Task<Guid?> Revoke(Guid transactionId, bool saveImmediately = true);
    public Task<Guid?> Revoke(Transaction? transaction, bool saveImmediately = true);
    public Task<Guid> Insert(Transaction transaction, bool saveImmediately = true);
}