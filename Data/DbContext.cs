using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Users.Models;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Submissions.Models;
using TraineeManagementApi.Reviews.Models;
using TraineeManagementApi.SubmissionFiles.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Trainee> Trainees 
    { 
        get; 
        set; 
    }

    public DbSet<User> Users 
    { 
        get; 
        set; 
    }

    public DbSet<Mentor> Mentors 
    { 
        get; 
        set; 
    }

    public DbSet<LearningTask> LearningTasks 
    { 
        get; 
        set; 
    }

    public DbSet<TaskAssignment> TaskAssignments 
    { 
        get; 
        set; 
    }

    public DbSet<Submission> Submissions 
    { 
        get; 
        set; 
    }

    public DbSet<Review> Reviews 
    { 
        get; 
        set; 
    }

    public DbSet<SubmissionFile> SubmissionFiles 
    { 
        get; 
        set; 
    }

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
            .Entries<ICreateTimestamp>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            entry.Entity.CreatedDate = DateTime.UtcNow;
        }

        var entries2 = ChangeTracker
            .Entries<IUpdateTimestamp>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries2)
        {
            entry.Entity.UpdatedDate = DateTime.UtcNow;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trainee>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Role)
                .HasConversion<string>();

            entity.HasIndex(u => u.Username)
                .IsUnique();
        });

        modelBuilder.Entity<Mentor>()
            .Property(m => m.Status)
            .HasConversion<string>();

        modelBuilder.Entity<LearningTask>()
            .Property(lt => lt.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasOne(tr => tr.Trainee)
                .WithMany(t => t.TaskAssignments)
                .HasForeignKey(tr => tr.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(me => me.Mentor)
                .WithMany(m => m.TaskAssignments)
                .HasForeignKey(me => me.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lt => lt.LearningTask)
                .WithMany(l => l.TaskAssignments)
                .HasForeignKey(lt => lt.LearningTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ta => ta.Status)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasOne(lt => lt.TaskAssignment)
                .WithMany(l => l.Submissions)
                .HasForeignKey(lt => lt.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(s => s.Status)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(lt => lt.Submission)
                .WithMany(l => l.Reviews)
                .HasForeignKey(lt => lt.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lt => lt.Mentor)
                .WithMany(l => l.Reviews)
                .HasForeignKey(lt => lt.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(r => r.ReviewStatus)
                .HasConversion<string>();
        });

        modelBuilder.Entity<SubmissionFile>(entity =>
        {
            entity.HasOne(s => s.Submission)
                .WithMany(su => su.SubmissionFiles)
                .HasForeignKey(su => su.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}