using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TraineeManagement.Api.CacheService;
using TraineeManagement.Api.CacheServiceInterface;
using TraineeManagement.Api.Data.DatabaseContext;
using TrainingDirectory.Api.DirectoryTraineeService;
using TrainingDirectory.Api.DirectoryTraineeServiceInterface;

var builder = WebApplication.CreateBuilder(args);

const string AllowedOriginsPolicy = "_myAllowSpecificOrigins";

// db connection
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
MySqlServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 46));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);


// redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    string configuration = builder.Configuration["Redis:ConnectionString"]!;
    return ConnectionMultiplexer.Connect(configuration);
});



string[] allowedOrigin = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigin.Length == 0)
{
    throw new InvalidOperationException(
        $"CORS configuration missing for environment: {builder.Environment.EnvironmentName}"
    );
}


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOriginsPolicy,
                      policy =>
                      {
                          policy.WithOrigins(allowedOrigin)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

builder.Services.AddSingleton<ICacheService,CacheService>();
builder.Services.AddScoped<IDirectoryTraineeService,DirectoryTraineeService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
