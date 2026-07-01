namespace TraineeManagement.Api.Data.Constants;

public static class AppConstants
{
    
    public static class Database
    {
        public static class MySqlErrorCodes
        {
            public const int NotFoundReference = 1452;
            public const int DeleteReference = 1451;
            public const int UsernameExists = 1062;
        }
    }

    public static class Security
    {
        public const string ClaimId = "Id"; 
        public const string ClaimUsername = "Username";
        public const string ClaimRole = "Role";
        public const string DefaultRole = "Admin";
        public const int DefaultExpiryMinutes = 60;

        public static class Seeding
        {
            public const string DefaultAdminUsername = "admin";
            public const string DefaultAdminPassword = "Admin@123";
        }
    }

    public static class ConfigSections
    {
        public const string FileStorage = "FileStorage";

        public const string GetRootPath = "RootPath:Path";

        public const string GetFrontendCors = "Cors:AllowedOrigins";
        
        public const string GetMicroServiceCors = "Cors:AllowedRequest";

        public const string GetRedisConnection = "Redis:ConnectionString";

        public const string GetRabbitMqSettings = "RabbitMQ";

        public const string GetDbConnection = "ConnectionStrings:DefaultConnection";

        public static readonly Version MySqlVersion = new Version(8, 0, 46);

        public const string GetMicroServiceUrl = "DirectoryService:BaseUrl";

        public const string GetMicroServiceHealthRoute = "/api/health";

        public const string HealthCheckLivenessRoute = "/health/live";

        public const string HealthCheckReadinessRoute = "/health/ready";

        public const string HealthReady = "ready";

        public const string HealthUnavailable = "unavailable";

         public const string GetJwt = "JWT";

        public const string JwtKey = "Key";

        public const string JwtIssuer = "Issuer";

        public const string JwtAudience = "Audience";

        public const string JwtExpiryMinute = "ExpiryMinutes";

        public const string ContentType = "application/json";

        public const string CorrelationIdHeaderName = "X-Correlation-ID";

    }

    public static class RabbitMQ
    {
        public static string SubmissionProcessing = "submission-processing";

        public static string GetExchange(string queueName) => $"{queueName}.exchange";
        public static string GetQueue(string queueName) => $"{queueName}.queue";
        public static string GetRoutingKey(string queueName) => $"{queueName}.routing-key";
        
        public static string GetDlxExchange(string queueName) => $"{queueName}.dlx";
        public static string GetDlxQueue(string queueName) => $"{queueName}.failed";
        public static string GetDlxRoutingKey(string queueName) => $"{queueName}.failed.routing-key";
    }

}