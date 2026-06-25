using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TraineeManagement.Api.GlobalExceptionMiddleware;
using TraineeManagement.Api.UserSeeder;
using Newtonsoft.Json;
using TraineeManagement.Api.FileStoreValidation;
using StackExchange.Redis;
using TraineeManagement.Api.Messaging.RabbitMQPublisher;
using TraineeManagement.Api.Messaging.RabbitMqConnectionSettings;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Messaging.RabbitMqConnection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


// for react cors
const string AllowedOriginsPolicy = "_myAllowSpecificOrigins";


// structured logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


// to allow the ModelState Validation instead of [ApiController]
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });


// to do not allow any extra field with new name and value
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
       options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error; 
    });


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


// rabbitMQ connection
builder.Services.AddSingleton<RabbitConnection>();

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMqService>();


// to validate the bearer token
IConfigurationSection jwtSettings = builder.Configuration.GetSection("JWT");
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


builder.Services.Configure<CustomFileStoreValidation>(
    builder.Configuration.GetSection(AppConstants.ConfigSections.FileStorage)
);


// Add your application services cleanly
builder.Services.AddApplicationServices();


// react origin
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


builder.Services.AddHttpContextAccessor();

WebApplication app = builder.Build();


// establish connection for the rabbit
using (IServiceScope scope = app.Services.CreateScope())
{
    RabbitConnection conn = scope.ServiceProvider.GetRequiredService<RabbitConnection>();
    await conn.InitializeAsync();
}

// seed the user
await UserSeeder.SeedAsync(app.Services);


app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(AllowedOriginsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();