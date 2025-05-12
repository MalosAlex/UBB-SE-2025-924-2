using BusinessLayer.Services.Interfaces;
using BusinessLayer.Services;
using Microsoft.EntityFrameworkCore;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Repositories;
using BusinessLayer.DataContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Data.SqlClient;
using BusinessLayer.Services.Proxies;
using Microsoft.OpenApi.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection with better error handling
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Using connection string: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });

    // Add detailed logging for database issues
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .EnableDetailedErrors()
               .LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Add JWT authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SteamWebApi",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SteamProfile",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourTemporarySecretKeyHere32CharsMini")),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var sessionId = context.Principal.FindFirst("sessionId")?.Value;
            if (!string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out var sessionGuid))
            {
                var sessionService = context.HttpContext.RequestServices.GetRequiredService<ISessionService>();
                sessionService.RestoreSessionFromDatabase(sessionGuid);
            }
        }
    };
});

// Add services to the container.
builder.Services.AddControllers();

// Register core services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// Register all other services
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICollectionsService, CollectionsService>();
builder.Services.AddScoped<IFeaturesService, FeaturesService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IFriendsService, FriendsService>();
builder.Services.AddScoped<IAchievementsService, AchievementsService>();
builder.Services.AddScoped<IOwnedGamesService, OwnedGamesService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Register core repositories
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Register all other repositories
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ICollectionsRepository, CollectionsRepository>();
builder.Services.AddScoped<IFeaturesRepository, FeaturesRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IFriendshipsRepository, FriendshipsRepository>();
builder.Services.AddScoped<IAchievementsRepository, AchievementsRepository>();
builder.Services.AddScoped<IOwnedGamesRepository, OwnedGamesRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IUserProfilesRepository, UserProfilesRepository>();

// Add test connection validation
builder.Services.AddHealthChecks()
    .AddCheck("database", () =>
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message);
        }
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with authentication support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Steam Web API",
        Version = "v1",
        Description = "API for Steam-like application"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS support for client applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steam Web API v1");
        c.RoutePrefix = "swagger";
        // Add authorization option in the UI
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Hide models by default
    });

    // Add database connection test endpoint for easy validation
    app.MapGet("/api/test/db", async (ApplicationDbContext dbContext) =>
    {
        try
        {
            // Try to connect by performing a simple query
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (canConnect)
            {
                return Results.Ok(new { Status = "Connected", Message = "Database connection successful" });
            }
            else
            {
                return Results.Problem("Could not connect to database");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem($"Database error: {ex.Message}");
        }
    });

    // Add simple test endpoint without database dependency
    app.MapGet("/api/test/ping", () =>
    {
        return Results.Ok(new
        {
            Status = "Success",
            Message = "API is running",
            Timestamp = DateTime.Now
        });
    });
}

// Add health checks endpoint 
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Add authentication middleware before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Try to validate database connection on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Try to connect to database
    if (await dbContext.Database.CanConnectAsync())
    {
        Console.WriteLine("DATABASE CONNECTION SUCCESSFUL");
    }
    else
    {
        Console.WriteLine("DATABASE CONNECTION FAILED - but did not throw exception");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"DATABASE CONNECTION ERROR: {ex.Message}");
    // Continue running the application even if database connection fails
}

app.Run();