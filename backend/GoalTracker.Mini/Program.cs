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
    db.Goals.Add(goal);
    await db.SaveChangesAsync();

    return Results.Created($"/goal/{goal.Id}", goal);
});

// ----------------------
// New: GET/goals -> get goal list in SQL Server via EF Core
// ----------------------

app.MapGet("/goals", async (
    AppDbContext db,
    CancellationToken ct) =>
{
    var items = await db.Goals
    .OrderBy(g => g.Id)
    .Select(g => new GoalDto(g.Id, g.Title, g.CreatedAt))
    .ToListAsync(ct);

    return Results.Ok(items);
})
.Produces<List<GoalDto>>(StatusCodes.Status200OK)
.WithName("GetGoals")
.WithOpenApi();

// ----------------------
// New: GET/goals -> get a goal by Id in SQL Server via EF Core
// ----------------------
app.MapGet("/goals/{id:int}", async ( int id,
    AppDbContext db,
    CancellationToken ct) =>
{
    var g  = await db.Goals
    .Where(x=> x.Id == id)
    .Select( x=> new GoalDto(x.Id, x.Title, x.CreatedAt))
    .ToListAsync(ct);
    return g is null ? Results.NotFound() : Results.Ok(g);
    
})
.Produces<GoalDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetGoalById")
.WithOpenApi();

// ----------------------
// New: Delete/goals -> delete a goal by id in SQL Server via EF Core
// ----------------------
app.MapDelete("/goals/{id:int}", async (int id,
     AppDbContext db,
     CancellationToken ct) => {
     var g = await db.Goals.FindAsync(id);
         if (g is null)
             return Results.NotFound();
         db.Goals.Remove(g);
         await db.SaveChangesAsync();

         return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithName("DeleteGoalById")
.WithOpenApi();

app.Run();

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}

public record GoalDto(int Id, string Title, DateTime CreatedAt);
