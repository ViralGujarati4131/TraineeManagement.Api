using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.FileStoreValidation;

namespace TraineeManagement.Api.Configuration;

public static class FileStoreConfig
{
    public static IServiceCollection AddFileStoreConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CustomFileStoreValidation>(
            configuration.GetSection(AppConstants.ConfigSections.FileStorage)
        );

        return services;
    }
}