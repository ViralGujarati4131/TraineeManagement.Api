using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TraineeManagementApi.GlobalExceptionMiddleware;
using TraineeManagementApi.LearningTasks.Service;
using TraineeManagementApi.LearningTasks.ServiceInterface;
using TraineeManagementApi.Mentors.Service;
using TraineeManagementApi.Mentors.ServiceInterface;
using TraineeManagementApi.Trainees.Service;
using TraineeManagementApi.Trainees.ServiceInterface;
using TraineeManagementApi.Users.Service;
using TraineeManagementApi.Users.ServiceInterface;
using TraineeManagementApi.Utils.JwtService;
using TraineeManagementApi.Utils.UserSeeder;
using TraineeManagementApi.TaskAssignments.ServiceInterface;
using TraineeManagementApi.TaskAssignments.Service;
using TraineeManagementApi.Submissions.ServiceInterface;
using TraineeManagementApi.Submissions.Service;
using TraineeManagementApi.Reviews.ServiceInterface;
using TraineeManagementApi.Reviews.Service;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
MySqlServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 46));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        OpenApiSecurityScheme scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token directly"
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", scheme);
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });
        return Task.CompletedTask;
    });
});

IConfigurationSection jwtSettings = builder.Configuration.GetSection("Jwt");
string jwtKeyString = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
byte[] tokenSigningKey = Encoding.UTF8.GetBytes(jwtKeyString);

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
builder.Services.AddScoped<IMentorServices, MentorService>();
builder.Services.AddScoped<ILearningTaskService, LearningTaskService>();
builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

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

WebApplication app = builder.Build();

await UserSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(AllowedOriginsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();