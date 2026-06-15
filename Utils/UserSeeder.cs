using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Users.Models;

namespace TraineeManagementApi.Utils.UserSeeder;

public class UserSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger<UserSeeder> _logger = scope.ServiceProvider.GetRequiredService<ILogger<UserSeeder>>();

        const string DefaultUser = "admin";
        try
        {
            _logger?.LogDebug("Checking whether the admin user {Username} already exists", DefaultUser);
            bool userExists = await db.Users.AnyAsync(u => u.Username == DefaultUser);
            if (!userExists)
            {
                _logger?.LogInformation("Starting database seeding: Creating admin user {Username}", DefaultUser);

                User adminUser = new User
                {
                    Username = DefaultUser,
                    Role = UserRole.Admin
                };

                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

                db.Users.Add(adminUser);
                await db.SaveChangesAsync();

                _logger?.LogInformation("Admin user {Username} seeded successfully", DefaultUser);
            }

            if (!await db.Trainees.AnyAsync())
            {
                db.Trainees.AddRange(
                    new Trainee { FirstName = "Viral", LastName = "Gujarati", Email = "viralgujarati@gmail.com", TechStack = "ReactNative", Status = TraineeStatus.Active },
                    new Trainee { FirstName = "Ravi", LastName = "Morabiya", Email = "ravimorabiya@gmail.com", TechStack = "Html, Css", Status = TraineeStatus.Inactive },
                    new Trainee { FirstName = "Harsh", LastName = "Nagdev", Email = "harshnagdev@gmail.com", TechStack = "DSA", Status = TraineeStatus.Completed }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Mentors.AnyAsync())
            {
                db.Mentors.AddRange(
                    new Mentor { FirstName = "Narendra", LastName = "Patel", Email = "nmp@gmail.com", Expertise = "OS, COA, CPP", Status = MentorStatus.Active },
                    new Mentor { FirstName = "Divyang", LastName = "Chauhan", Email = "divyang@gmail.com", Expertise = "Cyber Security", Status = MentorStatus.Inactive },
                    new Mentor { FirstName = "Vimal", LastName = "Pambhar", Email = "vimalpambhar@gmail.com", Expertise = "DSA", Status = MentorStatus.Active }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.LearningTasks.AnyAsync())
            {
                db.LearningTasks.AddRange(
                    new LearningTask { Title = "Backend", Description = "APIs, Controller, Sevice, Dto, Middleware", ExpectedTechStack = "ASP .NET Web API", DueDate = new DateOnly(2025, 6, 24), Status = LearningTaskStatus.Draft },
                    new LearningTask { Title = "Fronend", Description = "Components, Hooks, Methods", ExpectedTechStack = "React", DueDate = new DateOnly(2025, 6, 24), Status = LearningTaskStatus.Published },
                    new LearningTask { Title = "Database", Description = "CRUD, primary key, foreign key, Unique, Auto generated", ExpectedTechStack = "MySQL Server", DueDate = new DateOnly(2025, 6, 24), Status = LearningTaskStatus.Closed }
                );
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while seeding the admin user {Username}", DefaultUser);
        }
    }
}