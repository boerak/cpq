using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Cpq.Api.Data;
using Cpq.Api.Data.Seeding;
using Cpq.Api.Exceptions;
using Cpq.Api.Middleware;
using Cpq.Api.Services.Bom;
using Cpq.Api.Services.Configuration;
using Cpq.Api.Services.Mes;
using Cpq.Api.Services.Rules;
using Cpq.Api.Services.Specs;
using Cpq.Api.Services.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;

// Bootstrap logger for startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CPQ API");

    var builder = WebApplication.CreateBuilder(args);

    // ===== Serilog =====
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Cpq.Api");
    });

    // ===== Database =====
    builder.Services.AddDbContext<CpqDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
            });
    });

    // ===== Caching =====
    builder.Services.AddMemoryCache();

    // ===== RulesEngine options =====
    builder.Services.Configure<RulesEngineOptions>(
        builder.Configuration.GetSection(RulesEngineOptions.SectionName));

    // ===== Polly policies for RulesEngineClient =====
    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt - 1)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Log.Warning("Retry {RetryAttempt} for rules engine after {Delay}ms. Reason: {Reason}",
                    retryAttempt, timespan.TotalMilliseconds,
                    outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
            });

    var circuitBreakerPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, breakDelay) =>
            {
                Log.Warning("Rules engine circuit breaker OPENED for {BreakDelay}s. Reason: {Reason}",
                    breakDelay.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
            },
            onReset: () => Log.Information("Rules engine circuit breaker RESET"));

    var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

    // ===== HTTP Clients =====
    var rulesEngineSection = builder.Configuration.GetSection(RulesEngineOptions.SectionName);
    var rulesEngineBaseUrl = rulesEngineSection["BaseUrl"] ?? "http://gorules-agent:8080";
    var timeoutSeconds = int.TryParse(rulesEngineSection["TimeoutSeconds"], out var ts) ? ts : 10;

    builder.Services.AddHttpClient<IRulesEngineClient, RulesEngineClient>(client =>
    {
        client.BaseAddress = new Uri(rulesEngineBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddPolicyHandler(combinedPolicy);

    // ===== Application Services =====
    builder.Services.AddScoped<IProductSpecRepository, ProductSpecRepository>();
    builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
    builder.Services.AddScoped<IBomService, BomService>();
    builder.Services.AddScoped<SkuResolver>();
    builder.Services.AddScoped<SelectionValidator>();
    builder.Services.AddScoped<IMesExporter, FileMesExporter>();
    builder.Services.AddScoped<MesExportService>();

    // ===== Controllers =====
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // ===== CORS (dev — allow all) =====
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // ===== Rate Limiter =====
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("configuration-patch", limiterOptions =>
        {
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromSeconds(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ===== Health Checks =====
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready" })
        .AddUrlGroup(
            uri: new Uri($"{rulesEngineBaseUrl}/api/health"),
            name: "gorules-agent",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ready" });

    // ===== Exception handling =====
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // ===== Middleware pipeline =====
    app.UseExceptionHandler();

    app.UseCorrelationId();
    app.UseRequestLogging();

    app.UseCors();
    app.UseRateLimiter();

    app.MapControllers();

    // Health endpoints
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            });
            await context.Response.WriteAsync(result);
        }
    });

    // ===== Database migration and seeding =====
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<CpqDbContext>();
        var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<CpqDbContext>>();

        try
        {
            Log.Information("Applying database schema...");
            // Use MigrateAsync if migrations exist, otherwise EnsureCreated for initial PoC setup.
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await db.Database.MigrateAsync();
                Log.Information("Database migrations applied.");
            }
            else
            {
                // No EF migrations present yet — create schema directly (PoC)
                await db.Database.EnsureCreatedAsync();
                Log.Information("Database schema ensured.");
            }

            Log.Information("Seeding database...");
            await DataSeeder.SeedAllAsync(db, seedLogger);
            Log.Information("Database seeding complete.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to apply migrations or seed database");
            // Don't throw — allow app to start, health checks will reflect DB state
        }
    }

    Log.Information("CPQ API started successfully");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "CPQ API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
