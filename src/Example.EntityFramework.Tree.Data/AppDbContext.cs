using Example.EntityFramework.Tree.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.EntityFramework.Tree.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
    {

    }

    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var thisAssembly = GetType().Assembly;

        modelBuilder.ApplyConfigurationsFromAssembly(thisAssembly);
    }
}
