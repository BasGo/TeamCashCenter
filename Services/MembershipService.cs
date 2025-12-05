using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class MembershipService(ILogger<MembershipService> logger, IUserService userService, CashCenterContext Db) : IMembershipService
{
    public async Task<Dictionary<User, AffectedPayments>> SaveMembership(Membership ms, Dictionary<User, AffectedPayments>? affectedPayments = null)
    {
        var result = affectedPayments ?? new Dictionary<User, AffectedPayments>();
        var role = await userService.GetRoleById(ms.RoleId);
        if  (role == null || string.IsNullOrWhiteSpace(role.Name)) return result;
        
        var users = await userService.GetUsersInRoleAsync(role.Name);
        foreach (var user in users)
        {
            result = await InnerSaveMembership(ms, user, result);
        }

        return result;
    }

    public async Task<Membership?> GetByIdAsync(Guid id)
    {
        return await Db.Memberships.FindAsync(id);
    }

    public async Task<List<Membership>> GetActiveMemberships(Guid roleId)
    {
        return await Db.Memberships
            .Where(m => m.RoleId == roleId && m.StartDate <= DateTime.Today && m.EndDate >= DateTime.Today)
            .ToListAsync();
    }
    public async Task<Dictionary<User, AffectedPayments>> InnerSaveMembership(Membership ms, User user, Dictionary<User, AffectedPayments>? affectedPayments = null)
    {
        var result = affectedPayments ?? new Dictionary<User, AffectedPayments>();
        if (!result.TryGetValue(user, out var payments))
        {
            payments = new AffectedPayments();
            result.Add(user, payments);
        }

        var isNew = ms.Id == Guid.Empty;
        Membership? before = null;
        if (!isNew)
        {
            before = await Db.Memberships.AsNoTracking().FirstOrDefaultAsync(m => m.Id == ms.Id);
        }

        var typeFee = await Db.TransactionTypes.FirstOrDefaultAsync(t => t.IsRegularFee);

        if (isNew) Db.Memberships.Add(ms);
        else Db.Memberships.Update(ms);
        await Db.SaveChangesAsync();

        // Determine range for payments
        var start = ms.StartDate.Date;
        var end = ms.EndDate.Date;

        if (end < start) return result;
        
        var due = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);

        // Add missing planned payments for months in the range
        while (due <= last)
        {
            var exists = await Db.Payments.AnyAsync(pp =>
                pp.UserId == user.Id && pp.MembershipId == ms.Id && pp.DueDate.Year == due.Year && pp.DueDate.Month == due.Month);
            if (!exists)
            {
                var newPP = new Payment
                {
                    Description = $"Mitgliedsbeitrag {due:yyyy-MM}",
                    Amount = ms.MonthlyFee,
                    MembershipId = ms.Id,
                    TransactionTypeId = typeFee?.Id,
                    DueDate = due,
                    UserId = user.Id,
                    IsPaid = false
                };

                Db.Payments.Add(newPP);
                payments.Added.Add(newPP);
                logger.LogInformation("Added planned payment for player {PlayerId} due {Due}: {Amount}", user.Id,
                    due, ms.MonthlyFee);
            }

            due = due.AddMonths(1);
        }

        // If fees changed, update unpaid planned payments in range
        if (before != null)
        {
            var unpaid = await Db.Payments.Where(pp =>
                pp.UserId == user.Id && pp.MembershipId == ms.Id && !pp.IsPaid && pp.DueDate >= start && pp.DueDate <= last).ToListAsync();
            foreach (var pp in unpaid)
            {
                var old = pp.Amount;
                if (pp.Amount != ms.MonthlyFee)
                {
                    pp.Amount = ms.MonthlyFee;
                    Db.Payments.Update(pp);
                    payments.Updated.Add(pp);
                    logger.LogInformation("Updated planned payment {Id} for player {PlayerId} from {Old} to {New}",
                        pp.Id, pp.UserId, old, pp.Amount);
                }
            }
        }

        // If EndDate shortened, remove unpaid planned payments after the new end
        if (before != null && before.EndDate > ms.EndDate)
        {
            var cutoff = new DateTime(ms.EndDate.Year, ms.EndDate.Month, 1).AddMonths(1).AddDays(-1);
            var toRemove = await Db.Payments
                .Where(pp => pp.UserId == user.Id && pp.MembershipId == ms.Id && !pp.IsPaid && pp.DueDate > cutoff).ToListAsync();
            foreach (var pp in toRemove)
            {
                payments.Removed.Add(pp);
                Db.Payments.Remove(pp);
                logger.LogInformation("Removed planned payment {Id} for player {PlayerId} due {Due}", pp.Id,
                    pp.UserId, pp.DueDate);
            }
        }

        await Db.SaveChangesAsync();

        return result;
    }

    public class AffectedPayments
    {
        public List<Payment> Added { get; set; } = [];
        public List<Payment> Updated { get; set; } = [];
        public List<Payment> Removed { get; set; } = [];
    }

    public async Task<AffectedPayments> EnsurePaymentsForUser(Membership ms, User user)
    {
        var dict = await InnerSaveMembership(ms, user, null);
        if (dict != null && dict.TryGetValue(user, out var payments))
        {
            return payments;
        }

        return new AffectedPayments();
    }
}