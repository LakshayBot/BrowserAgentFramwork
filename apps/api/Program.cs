using System.Reflection;
using System.Text;
using BrowserAgent.Api.Api;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Infrastructure.Data;
using BrowserAgent.Api.Infrastructure.Encryption;
using BrowserAgent.Api.Infrastructure.Services;
using BrowserAgent.Api.Browser.Interfaces;
using BrowserAgent.Api.Infrastructure.AI;
using BrowserAgent.Api.Infrastructure.Storage;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BrowserAgent.Api")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new ApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = e.ErrorMessage,
                    Field = x.Key
                }))
                .ToList();

            var response = ApiResponse<object>.Fail(errors);
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Browser Agent API",
        Version = "v1",
        Description = "API for the Browser Agent Framework - an open-source platform for AI-powered browser automation."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=browser_agent;Username=browser_agent;Password=browser_agent";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
        npgsqlOptions.EnableRetryOnFailure(3);
    }));

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSecret = jwtSection["Secret"] ?? "CHANGE_ME_USE_A_SECURE_KEY_IN_PRODUCTION_32CHARS";
var jwtIssuer = jwtSection["Issuer"] ?? "browser-agent-api";
var jwtAudience = jwtSection["Audience"] ?? "browser-agent-web";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var encryptionConfigKey = builder.Configuration["Encryption:Key"];
var encryptionKey = string.IsNullOrEmpty(encryptionConfigKey)
    ? Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
    : encryptionConfigKey;

builder.Services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddSingleton<IWorkflowEventBus, WorkflowEventBus>();
builder.Services.AddSingleton<BrowserAgent.Api.Infrastructure.Services.HumanVerificationDetector>();
builder.Services.AddSingleton<BrowserAgent.Api.Plugins.JobApplication.JobApplicationPlugin>();
builder.Services.AddSingleton<BrowserAgent.Api.Plugins.PluginLoader>(sp =>
{
    var plugins = new List<BrowserAgent.Api.Plugins.IWorkflowPlugin>
    {
        sp.GetRequiredService<BrowserAgent.Api.Plugins.JobApplication.JobApplicationPlugin>()
    };
    return new BrowserAgent.Api.Plugins.PluginLoader(plugins);
});

builder.Services.AddSingleton<BrowserAgent.Api.Browser.BrowserManager.PlaywrightSessionManager>();
builder.Services.AddSingleton<IBrowserManager>(sp => sp.GetRequiredService<BrowserAgent.Api.Browser.BrowserManager.PlaywrightSessionManager>());
builder.Services.AddSingleton<INavigationService, BrowserAgent.Api.Browser.Navigation.NavigationService>();
builder.Services.AddSingleton<IFormExtractor, BrowserAgent.Api.Browser.Extraction.FormExtractor>();
builder.Services.AddSingleton<IInteractionService, BrowserAgent.Api.Browser.Interaction.InteractionService>();
builder.Services.AddSingleton<IScreenshotService, BrowserAgent.Api.Browser.Screenshot.ScreenshotService>();

var storagePath = builder.Configuration["Storage:LocalPath"]
    ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
builder.Services.AddSingleton<IStorageService>(new LocalStorageService(storagePath));

builder.Services.AddHttpClient("DeepSeek", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient("Ollama", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

var aiServiceBaseUrl = builder.Configuration["AIService:BaseUrl"] ?? "http://browser-agent-ai:8000";
builder.Services.AddHttpClient<IAiClient, AiClient>(client =>
{
    client.BaseAddress = new Uri(aiServiceBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgresql",
        tags: new[] { "database" })
    .AddUrlGroup(
        new Uri(builder.Configuration["AIService:BaseUrl"] ?? "http://localhost:8000/health"),
        name: "ai-service",
        tags: new[] { "external" });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Cors:Origins"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<BrowserAgent.Api.Api.Middleware.ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Browser Agent API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

try
{
    Log.Information("Starting Browser Agent API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
