using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public static class ServiceRegistry
{
    public static IServiceCollection AddCashCenterServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}