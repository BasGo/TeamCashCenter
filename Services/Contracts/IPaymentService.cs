using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services.Contracts;

public interface IPaymentService
{
    public Task UpsertAsync(Payment payment, bool saveImmediately = true);
    public Task CreateAsync(Payment payment, bool saveImmediately = true);
    public Task UpdateAsync(Payment payment, bool saveImmediately = true);
}