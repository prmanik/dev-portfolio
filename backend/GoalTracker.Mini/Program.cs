using FluentValidation;
using GoalTracker.Mini.Data;
using GoalTracker.Mini.DTOs;
using GoalTracker.Mini.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Add services to the container
// ----------------------
// 1) Wire EF Core with SQL Server using the connection string from appsettings.json

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) FluentValidation (auto-discovers validators in this assembly)
builder.Services.AddValidatorsFromAssemblyContaining<CreateGoal>();

// 3) Swagger (if you added OpenAPI/Swashbuckle for .NET 8)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();


// ----------------------
// HTTP request pipeline
// ----------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
}

//app.UseHttpsRedirection();


// ----------------------
// Smaple Minimal API
// ----------------------
//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast =  Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");


// ----------------------
// New: POST /goals -> create goal in SQL Server via EF Core
// ----------------------
app.MapPost("/goals", async (
    CreateGoal dto,
    IValidator<CreateGoal> validator,
    AppDbContext db) =>
{
    var validation = validator.Validate(dto);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());
    var goal = new Goal { Title = dto.Title };
    await db.SaveChangesAsync();

    return Results.Created($"/goal/{goal.Id}", goal);
});

app.Run();

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
