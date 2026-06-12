using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TraineeManagementApi.Service;
using TraineeManagementApi.Service.Interface;
using Users.Service;
using Users.Service.Interface;
using Users.Utils;

var builder = WebApplication.CreateBuilder(args);

const string AllowedOriginsPolicy = "_myAllowSpecificOrigins";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
        );
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT token directly"
        };
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", scheme);
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });
        return Task.CompletedTask;
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKeyString = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
var tokenSigningKey = Encoding.UTF8.GetBytes(jwtKeyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(tokenSigningKey)
    };
});

builder.Services.AddScoped<ITraineeService, TraineeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOriginsPolicy,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database seeding process...");
        await UserSeeder.SeedAsync(scope.ServiceProvider);
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseCors(AllowedOriginsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();