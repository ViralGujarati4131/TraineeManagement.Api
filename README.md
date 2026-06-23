# TraineeManagement.Api

---

## Technology Stack

| Layer | Technology |
|---|---|
| Language | C#(.NET 9) |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core (Code First) |
| Database | MySQL |
| Authentication | JWT Bearer Token |
| Password Hashing | ASP.NET Core PasswordHasher |
| Documentation | Swagger / OpenAPI |
| Logging | ASP.NET Core Built-in Logging |

---

## Project Structure

```
TraineeManagement.Api/
├── Constants/
│   ├── ApiResponseDescriptor.cs
│   ├── AppConstants.cs
├── Controllers/
│   ├── HealthController.cs
│   ├── TraineesController.cs
│   ├── UserController.cs
│   ├── MentorsController.cs
│   ├── LearningTasksController.cs
│   ├── TaskAssignmentsController.cs
│   ├── SubmissionsController.cs
│   ├── ReviewsController.cs
│   ├── SubmissionFilesController.cs
├── Models/
│   ├── Trainee.cs
│   ├── User.cs
│   ├── Mentor.cs
│   ├── LearningTask.cs
│   ├── TaskAssignment.cs
│   ├── Submission.cs
│   ├── Review.cs
│   ├── SubmissionFile.cs
├── DTOs/
│   ├── TraineeDto.cs
│   ├── UserDto.cs
│   ├── MentorDto.cs
│   ├── LearningTaskDto.cs
│   ├── TaskAssignmentDto.cs
│   ├── SubmissionDto.cs
│   ├── ReviewDto.cs
│   ├── SubmissionFileDto.cs
├── Interfaces/
│   ├── ILearningTaskServices.cs
│   ├── IMentorServices.cs
│   ├── ITimeStamp.cs
│   ├── ITraineeServices.cs
│   ├── IUserServices.cs
│   ├── ITaskAssignmentServices.cs
│   ├── ISubmissionServices.cs
│   ├── IReviewServices.cs
│   ├── IFileStorageServices.cs
│   ├── ISubmissionFileServices.cs
├── Services/
│   ├── TraineeServices.cs
│   ├── UserServices.cs
│   ├── MentorServices.cs
│   ├── LearningTaskServices.cs
│   ├── TaskAssignmentServices.cs
│   ├── SubmissionServices.cs
│   ├── ReviewServices.cs
│   ├── FileStorageServices.cs
│   ├── SubmissionFileServices.cs
├── Utils/
│   ├── CustomException.cs
│   ├── JwtService.cs
│   ├── UserSeeder.cs
│   ├── CustomValidation.cs
│   ├── FileStorageConfiguration.cs
│   ├── ResponseBuilder.cs
├── Middlewares/
│   └── GlobalExceptionMiddleware.cs
├── Data/
│   └── DbContext.cs
├── Migrations/
├── appsettings.json
└── Program.cs
```

---

## Backend Setup Steps

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server 8.x](https://dev.mysql.com/downloads/mysql/)

### 1. Clone the Repository

```bash
git clone https://github.com/ViralGujarati4131/TraineeManagement.Api
cd TraineeManagement.Api
```

### 2. Restore NuGet Packages Clean Project And Build It

```bash
dotnet restore
dotnet clean
dotnet build
```

---

## MySQL Setup Step

### Create the Database

Log in to MySQL and run:

```sql
CREATE DATABASE trainee_management_db;
```

---

## Configure `appsettings.Development.json`

Refer `appsettings.Development.template.json` and add accordingly your react cors, connection string, File configuration and jwt credential in `appsettings.Development.json`


## Configure `docker-compose.yml`

Refer `docker-compose.template.yml` and add the image of the redis, mysql and define port for mysql on your end and set database name also add your password credential in  `docker-compose.yml`


**Linux :** 
```bash
export ASPNETCORE_ENVIRONMENT=Development 
```

**Windows :** 
```bash
set ASPNETCORE_ENVIRONMENT=Development
```

---

## EF Core Migration Commands

```bash
dotnet ef database update
```

---

## Running the API

```bash
dotnet run
```

Swagger UI is available at:

```
https://localhost:<port>/swagger
```

---

## Login Credentials for Testing

Seed an Admin user during application startup.

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@123` |

---

## JWT Usage Instructions

### Step 1 — Login to Get Token

**POST** `/api/auth/login`

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response:**

```json
{
  "token": "<jwt-token-value>",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "admin",
    "role": "Admin"
  }
}
```

### Step 2 — Swagger UI (JWT Setup)

1. Click **Authorize** button in Swagger UI.
2. Enter: `Bearer <your-token>`
3. Click **Authorize**, then close the dialog.
4. All subsequent requests will include the token automatically.
 
---

## Sample Request & Response JSON

### Health Check

**GET** `/api/health`

```json
{
  "status": "running",
  "application": "Trainee Management API",
  "timestamp": "2026-06-11T10:30:00"
}
```

---

### Create Trainee

**POST** `/api/trainees`

Request:
```json
{
  "firstName": "Amit",
  "lastName": "Sharma",
  "email": "amit.sharma@training.com",
  "techStack": "HTML, CSS, JavaScript",
  "status": "Active"
}
```

Response `201 Created`:
```json
{
  "id": 1,
  "firstName": "Amit",
  "lastName": "Sharma"
}
```

---

### Get Trainees with Pagination

**GET** `/api/trainees?pageNumber=1&pageSize=10&search=amit&status=Active`

Response `200 OK`:
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 25,
  "data": [
    {
      "id": 1,
      "firstName": "Amit",
      "lastName": "Sharma"
    }
  ]
}
```

---

### Create Mentor

**POST** `/api/mentors`

Request:
```json
{
  "firstName": "Priya",
  "lastName": "Nair",
  "email": "priya.nair@company.com",
  "expertise": "C#, ASP.NET Core, SQL",
  "status": "Active"
}
```

---

### Create Learning Task

**POST** `/api/learning-tasks`

Request:
```json
{
  "title": "Build a REST API",
  "description": "Build a CRUD REST API using ASP.NET Core",
  "expectedTechStack": "C#, ASP.NET Core, EF Core",
  "dueDate": "2026-07-01",
  "status": "Published"
}
```
---

### Create Task Assignment
 
**POST** `/api/task-assignments`
 
Request:
```json
{
  "traineeId": 1,
  "mentorId": 1,
  "learningTaskId": 1,
  "assignedDate": "2026-06-11",
  "dueDate": "2026-07-01",
  "status": "Assigned",
  "remarks": "Complete Phase 2 API task"
}
```
 
---
 
### Submit Work
 
**POST** `/api/submissions`
 
Request:
```json
{
  "taskAssignmentId": 1,
  "submissionUrl": "https://github.com/trainee/trainee-management-api",
  "notes": "Completed all Phase 2 requirements including JWT and MySQL integration.",
  "status": "Submitted"
}
```
 
---
 
### Add Review
 
**POST** `/api/reviews`
 
Request:
```json
{
  "submissionId": 1,
  "mentorId": 1,
  "feedback": "Good work. Clean code structure and proper JWT implementation.",
  "score": 85,
  "reviewStatus": "Accepted"
}
```
 
---

### Add SubmissionFile
 
**POST** `/api/submission-files/1/files`
 
Request:
```
{
  "key": files,
  "value": <upload your files>
}
```

---

## CORS Configuration

CORS is configured to allow the React frontend origins:

- `http://localhost:3000`
- `http://localhost:5173`

---

## Security Checklist (OWASP API Security)

| Security Area | Implementation |
|---|---|
| Authentication | JWT bearer token validation enabled |
| Authorization | All APIs except `/api/health` and `/api/auth/login` require a valid token |
| Password storage | Passwords stored as hash only plain text never stored or logged |
| Excessive data exposure | DTOs used for all responses; `PasswordHash` never returned |
| Injection prevention | EF Core parameterized queries used; no raw unsafe SQL |
| Security misconfiguration | CORS restricted to known React dev origins |
| Logging | Passwords, tokens, and sensitive data never logged |

---



