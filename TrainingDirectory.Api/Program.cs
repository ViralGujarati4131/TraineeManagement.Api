using TrainingDirectory.Api.DirectoryTraineeService;
using TrainingDirectory.Api.DirectoryTraineeServiceInterface;
using TraineeManagement.Api.GlobalExceptionMiddleware;
using TraineeManagement.Api.CorrelationId;
using TraineeManagement.Api.Configuration;
using TraineeManagement.Api.CorrelationIdMiddleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
ILogger logger = loggerFactory.CreateLogger("Program");


builder.Services.AddMySqlConnection(builder.Configuration,logger);
builder.Services.AddMicroServiceCors(builder.Configuration,logger);
builder.AddSerilogLogging(); 
builder.Services.AddHttpContextAccessor();      

builder.Services.AddScoped<IDirectoryTraineeService,DirectoryTraineeService>();
builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);

WebApplication app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(SetMicroServiceCors.AllowedOriginsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();