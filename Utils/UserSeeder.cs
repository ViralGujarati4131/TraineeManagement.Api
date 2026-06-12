using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Users.Models;

public class UserSeeder
{
    private static ILogger<UserSeeder>? _logger;

    public UserSeeder(ILogger<UserSeeder> logger)
    {
        _logger = logger;
    }

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _logger ??= scope.ServiceProvider.GetService<ILogger<UserSeeder>>();

        const string DefaultUser = "admin";
        try
        {
            _logger?.LogDebug("Checking whether the admin user {Username} already exists", DefaultUser);
            
            var userExists = await db.Users.AnyAsync(u => u.Username == DefaultUser);
            if (!userExists)
            {
                _logger?.LogInformation("Starting database seeding: Creating admin user {Username}", DefaultUser);
                
                var adminUser = new User
                {
                    Username = DefaultUser,
                    Role = UserRole.Admin
                };
                
                var passwordHasher = new PasswordHasher<User>();
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
                
                await db.Users.AddAsync(adminUser);
                await db.SaveChangesAsync();
                
                _logger?.LogInformation("Admin user {Username} seeded successfully", DefaultUser);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while seeding the admin user {Username}", DefaultUser);
        }
    }
}