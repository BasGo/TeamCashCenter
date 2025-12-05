using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services.Contracts;

public interface IMembershipService
{
    public Task<Dictionary<User, MembershipService.AffectedPayments>> SaveMembership(Membership ms, Dictionary<User, MembershipService.AffectedPayments>? affectedPayments = null);
    public Task<Membership?> GetByIdAsync(Guid id);

    public Task<List<Membership>> GetActiveMemberships(Guid roleId);
    // Ensure planned payments exist for a single user when they join a role's membership
    public Task<MembershipService.AffectedPayments> EnsurePaymentsForUser(Membership ms, User user);
}