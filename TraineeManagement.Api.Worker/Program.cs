using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Messaging.RabbitMqConnectionSettings;
using TraineeManagement.Api.Worker.SubmissionProcessingConsumer;   
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using TraineeManagement.Api.CacheService;
using TraineeManagement.Api.CacheServiceInterface;
using StackExchange.Redis;
using TraineeManagement.Api.FileStoreValidation;
using TraineeManagement.Api.Data.Constants;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// mysql db connection
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
MySqlServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 46));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);

// get rabbitmq credentials
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

// dependancy Injections
builder.Services.AddSingleton<ICacheService, CacheService>();

// redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    string configuration = builder.Configuration["Redis:ConnectionString"]!;
    return ConnectionMultiplexer.Connect(configuration);
});

// rabbit connection
builder.Services.AddSingleton<RabbitConnection>();
builder.Services.AddHostedService<SubmissionProcessingConsumer>();


// get file store configurations
builder.Services.Configure<CustomFileStoreValidation>(
    builder.Configuration.GetSection(AppConstants.ConfigSections.FileStorage)
);

IHost host = builder.Build();

using (IServiceScope scope = host.Services.CreateScope())
{
    RabbitConnection conn = scope.ServiceProvider.GetRequiredService<RabbitConnection>();
    await conn.InitializeAsync();
}

await host.RunAsync();