using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.LearningTaskModel;
using TraineeManagement.Api.Data.MentorModel;
using TraineeManagement.Api.Data.TraineeModel;
using TraineeManagement.Api.Data.UserModel;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.DatabaseContext;

namespace TraineeManagement.Api.UserSeeder;

public class UserSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger<UserSeeder> logger = scope.ServiceProvider.GetRequiredService<ILogger<UserSeeder>>();

        try
        {
            logger.LogDebug("Checking whether the admin user already exists");
            
            bool userExists = await db.Users.AnyAsync(u => u.Username == AppConstants.Security.Seeding.DefaultAdminUsername);
            
            if (!userExists)
            {
                logger.LogInformation("Starting database seeding: Creating admin user");

                User adminUser = new User
                {
                    Username = AppConstants.Security.Seeding.DefaultAdminUsername,
                    Role = UserRole.Admin
                };

                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, AppConstants.Security.Seeding.DefaultAdminPassword);

                db.Users.Add(adminUser);
                await db.SaveChangesAsync();

                logger.LogInformation("Admin user seeded successfully");
            }
            
            if (!await db.Trainees.AnyAsync())
            {
                logger.LogInformation("Seeding default trainee directory metadata records.");
                db.Trainees.AddRange(
                    new Trainee { FirstName = "Viral", LastName = "Gujarati", Email = "viralgujarati@gmail.com", TechStack = "ReactNative", Status = TraineeStatus.Active },
                    new Trainee { FirstName = "Ravi", LastName = "Morabiya", Email = "ravimorabiya@gmail.com", TechStack = "Html, Css", Status = TraineeStatus.Inactive },
                    new Trainee { FirstName = "Harsh", LastName = "Nagdev", Email = "harshnagdev@gmail.com", TechStack = "DSA", Status = TraineeStatus.Completed }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Mentors.AnyAsync())
            {
                logger.LogInformation("Seeding default mentor directory metadata records.");
                db.Mentors.AddRange(
                    new Mentor { FirstName = "Narendra", LastName = "Patel", Email = "nmp@gmail.com", Expertise = "OS, COA, CPP", Status = MentorStatus.Active },
                    new Mentor { FirstName = "Divyang", LastName = "Chauhan", Email = "divyang@gmail.com", Expertise = "Cyber Security", Status = MentorStatus.Inactive },
                    new Mentor { FirstName = "Vimal", LastName = "Pambhar", Email = "vimalpambhar@gmail.com", Expertise = "DSA", Status = MentorStatus.Active }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.LearningTasks.AnyAsync())
            {
                logger.LogInformation("Seeding default learning syllabus task records.");
                db.LearningTasks.AddRange(
                    new LearningTask { Title = "Backend", Description = "APIs, Controller, Service, Dto, Middleware", ExpectedTechStack = "ASP .NET Web API", DueDate = new DateOnly(2026, 6, 24), Status = LearningTaskStatus.Draft },
                    new LearningTask { Title = "Frontend", Description = "Components, Hooks, Methods", ExpectedTechStack = "React", DueDate = new DateOnly(2026, 6, 24), Status = LearningTaskStatus.Published },
                    new LearningTask { Title = "Database", Description = "CRUD, primary key, foreign key, Unique, Auto generated", ExpectedTechStack = "MySQL Server", DueDate = new DateOnly(2026, 6, 24), Status = LearningTaskStatus.Closed }
                );
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure during user seeding routines.");
            
            throw new DataSeedingException();
        }
    }
}