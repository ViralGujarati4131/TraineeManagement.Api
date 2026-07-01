using TraineeManagement.Api.LearningTaskService;
using TraineeManagement.Api.LearningTaskServiceInterface;
using TraineeManagement.Api.MentorService;
using TraineeManagement.Api.MentorServiceInterface;
using TraineeManagement.Api.UserService;
using TraineeManagement.Api.UserServiceInterface;
using TraineeManagement.Api.TaskAssignmentServiceInterface;
using TraineeManagement.Api.TaskAssignmentService;
using TraineeManagement.Api.SubmissionServiceInterface;
using TraineeManagement.Api.SubmissionService;
using TraineeManagement.Api.ReviewServiceInterface;
using TraineeManagement.Api.ReviewService;
using TraineeManagement.Api.FileStorageServiceInterface;
using TraineeManagement.Api.FileStorageService;
using TraineeManagement.Api.SubmissionFileServiceInterface;
using TraineeManagement.Api.SubmissionFileService;
using TraineeManagement.Api.Data.CacheServiceInterface;
using TraineeManagement.Api.Data.CacheService;
using TraineeManagement.Api.JwtService;
using TraineeManagement.Api.ProcessingJobService;
using TraineeManagement.Api.ProcessingJobServiceInterface;
using TraineeManagement.Api.CorrelationId;
using TraineeManagement.Api.Messaging.RabbitMQPublisher;

namespace Microsoft.Extensions.DependencyInjection; 

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IMentorServices, MentorService>();
        services.AddScoped<ILearningTaskService, LearningTaskService>();
        services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ISubmissionFileService, SubmissionFileService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IProcessingJobService,ProcessingJobService>();
        services.AddScoped<ICorrelationIdAccessor,CorrelationIdAccessor>();
        services.AddSingleton<RabbitMqService>();

        return services;
    }
}