using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Helper;
using TeamCashCenter.Services;
using TeamCashCenter.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

// add blazor bootstrap
builder.Services.AddBlazorBootstrap();

// add logging
builder.Logging.ClearProviders().AddConsole();

// Configure Kestrel to listen on HTTP and HTTPS for local development.
// HTTPS will use the default development certificate (use `dotnet dev-certs https --trust` if needed).
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS (development cert)
    });
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure HttpClient BaseAddress from configuration `ServerBaseUrl` or fallback to localhost
var serverBase = builder.Configuration["ServerBaseUrl"];
if (string.IsNullOrWhiteSpace(serverBase)) serverBase = "https://localhost:5001/";
builder.Services.AddHttpClient("server", client => client.BaseAddress = new Uri(serverBase));
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("server"));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddControllers();

// Email options and sender
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();

// App-level options (display name etc.)
var appOptionsSection = builder.Configuration.GetSection("App");
builder.Services.Configure<AppOptions>(appOptionsSection);

builder.Services.AddDbContext<CashCenterContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=TeamCashCenter.db"));

builder.Services.AddIdentity<User, Role>(options => { options.SignIn.RequireConfirmedAccount = false; })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<CashCenterContext>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    // Policy to restrict who can view transactions - adjust roles as appropriate for your deployment
    options.AddPolicy("CanViewTransactions", policy => policy.RequireRole("Admin", "Manager", "Treasurer"));
});

// add application services
builder.Services.AddCashCenterServices();

// Use the default AuthenticationStateProvider provided by Blazor+Identity (no custom registration needed)

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Redirect HTTP to HTTPS where available
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var appOptions = appOptionsSection.Get<AppOptions>();

// Apply migrations and seed data on startup (safe for development)
using (var scope = app.Services.CreateScope())
{
    try
    {
        await scope.MigrateDatabaseAsync();

        await scope.SeedDataAsync();
        
        // ensure required roles exist
        await scope.CreateRoles();
        
        // create default admin user (requires Identity services)
        await scope.SeedAdminAsync(appOptions ?? new AppOptions());
    }
    catch (Exception ex)
    {
        // In production, you would log this
        Console.WriteLine("Migration/Seed failed: " + ex.Message);
    }
}

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Logger.LogInformation("Application started successfully");

app.Run();
