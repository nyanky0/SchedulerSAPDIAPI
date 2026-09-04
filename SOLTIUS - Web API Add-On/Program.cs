using System;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SOLTIUS_Web_API_Add_On.Authentication;
using SOLTIUS_Web_API_Add_On.Database.Factories;
using SOLTIUS_Web_API_Add_On.Database.Initializers;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Middleware;
using SOLTIUS_Web_API_Add_On.Models.Configuration;
using SOLTIUS_Web_API_Add_On.Repositories;
using SOLTIUS_Web_API_Add_On.Services;
using SOLTIUS_Web_API_Add_On.Services.AuditLog;
using SOLTIUS_Web_API_Add_On.Services.Configuration;
using SOLTIUS_Web_API_Add_On.Services.Status;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// P0: Kestrel limits
// ============================================================
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_048_576; // 1MB
});

builder.Services.AddControllers();

// ============================================================
// P0: Startup validation — reject if signing key is default
// ============================================================
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
JwtOptions jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) ||
    jwtOptions.SigningKey.Length < 32 ||
    jwtOptions.SigningKey.Contains("BABIBABIBABIBABIBABIBABIBABIBABIBABIBABI"))
{
    throw new InvalidOperationException(
        "JWT SigningKey is not configured. " +
        "Set environment variable JWT_SIGNING_KEY to a random string (min 32 chars).");
}

// Validate client secrets
if (jwtOptions.Clients != null)
{
    foreach (var client in jwtOptions.Clients)
    {
        if (string.IsNullOrWhiteSpace(client.ClientSecret) ||
            client.ClientSecret.Contains("BABIBABIBABIBABIBABIBABIBABIBABIBABIBABI"))
        {
            throw new InvalidOperationException(
                $"Client '{client.ClientId}' has a default ClientSecret. " +
                $"Set environment variable CLIENT_SECRET before starting.");
        }
    }
}

byte[] jwtSigningKey = System.Text.Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

// ============================================================
// JWT Auth
// ============================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSigningKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// Refresh Token Store (singleton, in-memory)
// ============================================================
int refreshDays = jwtOptions.RefreshTokenDays > 0 ? jwtOptions.RefreshTokenDays : 7;
builder.Services.AddSingleton(new RefreshTokenStore(refreshDays * 24 * 60));

// ============================================================
// P1: Rate Limiting (built-in .NET 8)
// ============================================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("api-limiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

// ============================================================
// P2: Health Checks (built-in)
// ============================================================
builder.Services.AddHealthChecks();

// ============================================================
// P1: Audit Log — Channel<T> + BackgroundService
// ============================================================
builder.Services.AddSingleton(Channel.CreateBounded<ApiLogEntry>(new BoundedChannelOptions(10000)
{
    FullMode = BoundedChannelFullMode.DropOldest
}));
builder.Services.AddSingleton(Channel.CreateBounded<SyncLogEntry>(new BoundedChannelOptions(10000)
{
    FullMode = BoundedChannelFullMode.DropOldest
}));
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();

// ============================================================
// P1: Log Flush Worker (reads from Channel, writes to DB log)
// ============================================================
string logConnStr = "";
try
{
    var logDbSection = builder.Configuration.GetSection("LogDatabase");
    var logDb = logDbSection.Get<LogDatabaseConfig>();
    if (logDb != null && !string.IsNullOrWhiteSpace(logDb.Server) &&
        !string.IsNullOrWhiteSpace(logDb.DatabaseName))
    {
        logConnStr = $"Server={logDb.Server};" +
            (logDb.Port > 0 ? $"Port={logDb.Port};" : "") +
            $"Database={logDb.DatabaseName};" +
            $"User Id={logDb.UserName};Password={logDb.Password};" +
            $"TrustServerCertificate=true;";
    }
}
catch { }

if (!string.IsNullOrWhiteSpace(logConnStr))
{
    builder.Services.AddSingleton(new LogDatabaseOptions { ConnectionString = logConnStr });
    builder.Services.AddHostedService<LogFlushWorker>();
}

// ============================================================
// Application Services
// ============================================================
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
builder.Services.AddSingleton<IDatabaseInitializerFactory, DatabaseInitializerFactory>();

builder.Services.AddTransient<MySqlDatabaseInitializer>();
builder.Services.AddTransient<SqlServerDatabaseInitializer>();

builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();

// ============================================================
// Swagger (P0: only in Development)
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Input JWT Bearer token only."
    });
});

// ============================================================
// Build
// ============================================================
var app = builder.Build();

// ============================================================
// Startup DB init (staging)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var cfgService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
    if (cfgService.ConfigExists())
    {
        try
        {
            var dbConfig = cfgService.GetDatabaseConfig();
            var factory = scope.ServiceProvider.GetRequiredService<IDatabaseInitializerFactory>();
            var initializer = factory.Create(dbConfig);
            await initializer.InitializeAsync(dbConfig);
        }
        catch { }
    }
}

// ============================================================
// Startup Log DB init
// ============================================================
if (!string.IsNullOrWhiteSpace(logConnStr))
{
    try
    {
        var logInitializer = new LogDatabaseInitializer(logConnStr);
        await logInitializer.InitializeAsync();
    }
    catch { }
}

// ============================================================
// Middleware pipeline (order matters)
// ============================================================
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// P1: Rate limiting
app.UseRateLimiter();

// P2: Health check
app.MapHealthChecks("/health");

app.MapControllers();

// P0: Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
