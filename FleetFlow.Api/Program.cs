
using FleetFlow.Application.Jobs;
using FleetFlow.Application.Jobs.CreateJob;
using FleetFlow.Application.Jobs.GetJob;
using FleetFlow.Application.Jobs.ChangeJobStatus;
using FleetFlow.Infrastructure.Persistence;
using FleetFlow.Application.Tenants;
using FleetFlow.Application.Tenants.CreateTenant;
using FleetFlow.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FleetFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FleetFlow")));

builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<CreateJobService>();
builder.Services.AddScoped<GetJobService>();

builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<CreateTenantService>();
builder.Services.AddScoped<ChangeJobStatusService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    IAuthorizationHandler,
    TenantAccessHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new TenantAccessRequirement());
    });

    options.AddPolicy("AdministratorOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Administrator");
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) =>
    {
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT signing key is not configured.");

        var keyBytes = Convert.FromBase64String(jwtKey);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must be at least 32 bytes.");
        }

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(keyBytes),

                RequireSignedTokens = true,
                RequireExpirationTime = true,

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });



// Add authorization services.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    return Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]))
        .ToArray();
})
.WithName("GetWeatherForecast");

app.MapGet("/api/auth/check", (
    System.Security.Claims.ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        Message = "Authentication successful",
        UserId = user.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
    });
})
.RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }