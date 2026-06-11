using Microsoft.EntityFrameworkCore;
using Users.Models;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlTypes;

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

        string defaultUser = "admin";
        try
        {
            _logger?.LogDebug("Check whether the admin user is already exists or not");
            if (!await db.Users.AnyAsync(u => u.Username == defaultUser))
            {
                _logger?.LogDebug("Start creating Admin seeder user");
                var adminUser = new User
                {
                    Username = defaultUser,
                    Role = UserRole.Admin
                };
                PasswordHasher<User> ps = new PasswordHasher<User>();
                adminUser.PasswordHash = ps.HashPassword(adminUser, "Admin@123");
                
                db.Users.Add(adminUser);
                await db.SaveChangesAsync();
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex);
        }

    }
}