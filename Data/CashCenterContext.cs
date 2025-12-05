using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Data;

public class CashCenterContext(DbContextOptions<CashCenterContext> options) : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<PaymentLog> PaymentLogs => Set<PaymentLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(u => u.ToTable("Users"));
        modelBuilder.Entity<Role>(u => u.ToTable("Roles"));
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(u => u.ToTable("RoleClaims"));
        modelBuilder.Entity<IdentityUserClaim<Guid>>(u => u.ToTable("UserClaims"));
        modelBuilder.Entity<IdentityUserLogin<Guid>>(u => u.ToTable("UserLogins"));
        modelBuilder.Entity<IdentityUserRole<Guid>>(u => u.ToTable("UserRoles"));
        modelBuilder.Entity<IdentityUserToken<Guid>>(u => u.ToTable("UserTokens"));
    }
}
