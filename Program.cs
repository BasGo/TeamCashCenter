using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Helper;
using TeamCashCenter.Services;
using TeamCashCenter.Services.Contracts;

using Serilog;
using System.Reflection;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// determine data directory (from environment or default)
var dataDir = builder.Configuration["DATA_DIR"] ?? Environment.GetEnvironmentVariable("DATA_DIR") ?? "./data";
Directory.CreateDirectory(dataDir);
var logDir = builder.Configuration["LOG_DIR"] ?? Environment.GetEnvironmentVariable("LOG_DIR") ?? "./logs";
Directory.CreateDirectory(logDir);

// common DB file path (used when no connection string provided)
var dbFilePath = Path.Combine(dataDir, "TeamCashCenter.db");

// configure Serilog to log to console and file in the mapped data folder
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .WriteTo.File(Path.Combine(logDir, "applog-.log"), rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Log resolved runtime paths early so they appear in boot logs
Log.Information("Resolved paths: DATA_DIR={DataDir}, LOG_DIR={LogDir}, DB_FILE={DbFile}", dataDir, logDir, dbFilePath);

// determine application version from assembly (informational version preferred)
var entryAssembly = Assembly.GetEntryAssembly();
var informationalVersion = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
var assemblyVersion = entryAssembly?.GetName()?.Version?.ToString();
var appVersion = informationalVersion ?? assemblyVersion ?? "unknown";

// add blazor bootstrap
builder.Services.AddBlazorBootstrap();

// existing console/file logging handled by Serilog

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

// Determine DB connection: prefer configured connection string, else place DB file in mapped data directory
var configured = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = configured;
if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbPath = Path.Combine(dataDir, "TeamCashCenter.db");
    var dbDir = Path.GetDirectoryName(dbPath) ?? dataDir;
    Directory.CreateDirectory(dbDir);
    connectionString = $"Data Source={dbPath}";
}
builder.Services.AddDbContext<CashCenterContext>(options =>
    options.UseSqlite(connectionString));

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

// Minimal /info endpoint for diagnostics
app.MapGet("/info", () => new
{
    version = appVersion,
    dataDir = dataDir,
    logDir = logDir,
    dbFile = dbFilePath
});

app.Logger.LogInformation("Application started successfully; DATA_DIR={DataDir}; LOG_DIR={LogDir}; DB_FILE={DbFile}", dataDir, logDir, dbFilePath);

app.Run();
