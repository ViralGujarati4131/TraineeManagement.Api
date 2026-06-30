using Serilog;
using Serilog.Events;

namespace TraineeManagement.Api.Configuration;

public static class SerilogConfiguration
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)  
            .Enrich.FromLogContext()                           
            .WriteTo.Conditional(
                evt => evt.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? src)
                       && src.ToString().Contains("RequestLoggingMiddleware"),
                wt => wt.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss}] [{Level:u3}] [HTTP] [Corr: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
            )
            .WriteTo.Conditional(
                evt => !(evt.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? src)
                         && src.ToString().Contains("RequestLoggingMiddleware")),
                wt => wt.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{SourceContext}] [Corr: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
            )
        );
    }
}