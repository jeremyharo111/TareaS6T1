using System.Reflection.Emit;
using backendapi.Models;
using Microsoft.EntityFrameworkCore;

namespace backendapi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
    }
}
