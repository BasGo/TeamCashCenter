using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services.Contracts;

public interface ITransferService
{
    public Task Upsert(Transfer? actual, Transfer? former = null);
}