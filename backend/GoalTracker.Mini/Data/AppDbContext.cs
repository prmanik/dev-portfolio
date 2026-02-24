using GoalTracker.Mini.Models;
using Microsoft.EntityFrameworkCore;

namespace GoalTracker.Mini.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {

    }

    public DbSet<Goal> Goals => Set<Goal>();
}