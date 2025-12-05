using TeamCashCenter.Data;

namespace TeamCashCenter.Helper;

public static class DataExtensions
{
    public static async Task<int> SaveChangesAsyncIf(this CashCenterContext db, bool saveChanges)
    {
        if (saveChanges)
        {
            return await db.SaveChangesAsync();
        }

        return 0;
    }
}