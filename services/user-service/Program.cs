using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Middleware;
using user_service.Repositories;
using user_service.Services;
using user_service.UnitOfWork;
using user_service.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000" };

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ITokenManagementService, TokenManagementService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? throw new InvalidOperationException("JWT SecretKey is not configured. Set JWT:SecretKey in appsettings.json or JWT_SECRET_KEY environment variable.");

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long for security reasons.");
        }

        var issuer = jwtSettings["Issuer"] ?? "spm-api-gateway";
        var audience = jwtSettings["Audience"] ?? "spm-services";

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handler middleware (should be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and migrated (for development)
// In production, run migrations separately
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        try
        {
            db.Database.EnsureCreated();
            // Uncomment to run migrations automatically:
            // db.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while creating/migrating the database.");
        }
    }
}

app.Run();

