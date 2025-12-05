using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Helper;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class PaymentService(CashCenterContext Db) : IPaymentService
{
    public async Task CreateAsync(Payment payment, bool saveImmediately = true)
    {
        if (payment.Id != Guid.Empty) return;
        Db.Payments.Add(payment);
        await Db.SaveChangesAsyncIf(saveImmediately);
    }
    
    public async Task UpdateAsync(Payment payment, bool saveImmediately = true)
    {
        if (payment.Id == Guid.Empty) return;
        Db.Payments.Update(payment);
        await Db.SaveChangesAsyncIf(saveImmediately);
    }
    
    public async Task UpsertAsync(Payment payment, bool saveImmediately = true)
    {
        if (payment.Id == Guid.Empty)
        {
            await CreateAsync(payment, saveImmediately);
            return;
        }
        
        await UpdateAsync(payment, saveImmediately);
    }
}