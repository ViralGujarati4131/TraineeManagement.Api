using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Messaging.RabbitMqConnectionSettings;
using TraineeManagement.Api.Worker.SubmissionProcessingConsumer;   
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using TraineeManagement.Api.CacheService;
using TraineeManagement.Api.CacheServiceInterface;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

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

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var conn = scope.ServiceProvider.GetRequiredService<RabbitConnection>();
    await conn.InitializeAsync();
}

await host.RunAsync();