namespace TraineeManagementApi.Constants
{
    public static class AppConstants
    {
        public static class ApiResponse
        {
            // Global Application Codes
            public const string CodeSuccess = "2000";
            public const string CodeCreated = "2010";
            public const string CodeBadRequest = "4000";
            public const string CodeValidationError = "4010";
            public const string CodeUnauthorized = "4030";
            public const string CodeNotFound = "4040";
            public const string CodeServerError = "5000";

            // Universal Shared Messages
            public const string MsgSuccess  = "Data retrieved successfully";
            public const string MsgCreated  = "Data created successfully";
            public const string MsgUpdated  = "Data updated successfully";
            public const string MsgDeleted  = "Data deleted successfully";
            public const string MsgNotFound = "Requested data was not found";
            public const string MsgValidationError = "Validation failed for the request data";
            public const string MsgBadRequest = "Invalid request arguments or operation rules";
        }

        public static class Errors
        {
            public const string JwtAuthError = "An unexpected error occurred while processing authentication please retry";
            public const string SqlReferenceConflict = "Some of the provided References does conflits ";
            public const string SqlDeleteReferenceError = "Delete Operation Could not be completed because of existing reference";
            public const string UsernameExists = "Username is already exists";
            public const string GeneralInternalServerError = "Something Went Wrong, Please Try Again";
            public const string JwtSecretMissing = "JWT Secret Key not configured.";

            public static class Users
            {
                public const string InvalidCredentials = "Invalid credential provided";
            }
        }

        public static class Routes
        {
            public const string LearningTasks = "api/learning-tasks";
            public const string Mentors = "api/mentors";
            public const string Reviews = "api/reviews";
            public const string Submissions = "api/submissions";
            public const string Trainees = "api/trainee";
            public const string TaskAssignments = "api/task-assignments";
            public const string Auth = "api/auth";
            public const string PaginationSearch = "paginationSearch";
            public const string Login = "login";
        }

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
    }
}