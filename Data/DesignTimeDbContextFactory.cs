using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TeamCashCenter.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CashCenterContext>
{
    public CashCenterContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CashCenterContext>();
        builder.UseSqlite("Data Source=mannschaftskasse.db");
        return new CashCenterContext(builder.Options);
    }
}
