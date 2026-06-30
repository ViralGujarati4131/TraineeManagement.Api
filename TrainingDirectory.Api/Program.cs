using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.DatabaseContext;
using TrainingDirectory.Api.DirectoryTraineeService;
using TrainingDirectory.Api.DirectoryTraineeServiceInterface;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.GlobalExceptionMiddleware;
using TraineeManagement.Api.CorrelationId;
using TraineeManagement.Api.Configuration;
using TraineeManagement.Api.CorrelationIdMiddleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
ILogger logger = loggerFactory.CreateLogger("Program");


const string AllowedOriginsPolicy = "_myAllowSpecificOrigins";

// db connection
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
MySqlServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 46));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);

string[]? allowedOrigins = builder.Configuration.GetSection("Cors:AllowedRequest").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    logger.LogCritical("Dependency failure: CORS are missing.");
    throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
}


logger.LogInformation("Configuring CORS policy. AllowedOriginsCount: {Count}", allowedOrigins.Length);
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOriginsPolicy,
                      policy =>
                      {
                          policy.WithOrigins(allowedOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

builder.Services.AddScoped<IDirectoryTraineeService,DirectoryTraineeService>();

builder.Services.AddHttpContextAccessor();      
builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
builder.AddSerilogLogging(); 

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

WebApplication app = builder.Build();


app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(AllowedOriginsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();