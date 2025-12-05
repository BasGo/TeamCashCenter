using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class TransferService(ILogger<TransferService> logger, ITransactionService transactionService, CashCenterContext Db) : ITransferService
{
    public async Task Upsert(Transfer? actual, Transfer? former = null)
    {
        if (actual == null) return;

        var ast = actual.CreateSourceTransaction();
        var att = actual.CreateTargetTransaction();
        
        if (former == null)
        {
            await transactionService.Insert(ast, false);
            await transactionService.Insert(att, false);

            var result = await Db.SaveChangesAsync();
            logger.LogInformation("Save changes had {Result} changes.", result);
            return;
        }

        
        // source changed -> revoke source transaction and insert current
        if (actual.SourceAccountId != former.SourceAccountId || actual.Amount != former.Amount)
        {
            logger.LogInformation("Source accounts different or amount changed.");
            var fst = await transactionService.GetAsync(former.Id, former.SourceAccountId);
            await transactionService.Revoke(fst, false);
            await transactionService.Insert(ast, false);

        }
        
        // target changed -> revoke target transaction and insert current
        if (actual.TargetAccountId != former.TargetAccountId || actual.Amount != former.Amount)
        {
            logger.LogInformation("Target accounts different or amount changed.");
            var ftt = await transactionService.GetAsync(former.Id, former.TargetAccountId);
            await transactionService.Revoke(ftt, false);
            await transactionService.Insert(att, false);

        }
        
        var result2 = await Db.SaveChangesAsync();
    }
}