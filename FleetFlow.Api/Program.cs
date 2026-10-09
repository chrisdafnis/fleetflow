
using FleetFlow.Application.Jobs;
using FleetFlow.Application.Jobs.CreateJob;
using FleetFlow.Application.Jobs.GetJob;
using FleetFlow.Application.Jobs.ChangeJobStatus;
using FleetFlow.Infrastructure.Persistence;
using FleetFlow.Application.Tenants;
using FleetFlow.Application.Tenants.CreateTenant;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}