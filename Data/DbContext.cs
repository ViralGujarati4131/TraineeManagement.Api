using Microsoft.EntityFrameworkCore;
using MyApi.Models;
using TraineeManagementApi.Models;
using Users.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Trainee> Trainees { get; set; }
    
    public DbSet<User> Users { get; set; }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTimestamps()
    {
        var entries = ChangeTracker
            .Entries<ITimestamp>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
            }
            entry.Entity.UpdatedDate = DateTime.UtcNow;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trainee>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}