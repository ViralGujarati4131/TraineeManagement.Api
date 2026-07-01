# Trainee Management API

A backend REST API for managing trainees, mentors, learning tasks, assignments, submissions, and reviews. Built with ASP.NET Core Web API and Entity Framework Core, it supports JWT authentication, file uploads, distributed caching with Redis, and asynchronous processing through RabbitMQ and a background worker service.

---

## Technology Stack

- **Language:** C# (.NET)
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core (Code First)
- **Database:** MySQL
- **Authentication:** JWT Bearer Tokens
- **Caching:** Redis
- **Messaging:** RabbitMQ
- **Background Processing:** .NET Worker Service
- **Containerization:** Docker Compose
- **API Documentation:** Postman

---

## Project Structure

```
TraineeManagement.Api/
├── Controllers/
├── DTOs/
├── Services/
├── Models/
├── Data/
│   └── AppDbContext.cs
├── Middleware/
├── Migrations/
├── appsettings.json
└── Program.cs

SubmissionProcessor.Worker/
└── Worker that consumes RabbitMQ messages and processes submission files

TrainingDirectory.Api/
└── Internal service that provides trainee and assignment profile data
```

---

## Prerequisites

Make sure the following are installed before running the project:

- [.NET SDK 9]() 
- [MySQL Server 8.0.x]()
- [Docker Compose in wsl]()
- [Postman]() 

---

## Setup and Running the Project

### Step 1 — Clone the Repository

```bash
git clone https://github.com/ViralGujarati4131/trainee-management-api.git
cd trainee-management.api
```

### Step 2 — Configure appsettings.json

Open `appsettings.json` and fill in your MySQL credentials and JWT settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=trainee_management_db;user=root;password=your_password;"
  },
  "Jwt": {
    "Issuer": "TraineeManagementApi",
    "Audience": "TraineeManagementClient",
    "ExpiryMinutes": 60
  }
}
```

> Do not hardcode the JWT signing key inside any controller or service. Keep it in configuration only.  
> Do not commit real passwords or secrets to source control.

### Step 3 — Run the API

```bash
dotnet run
```

Once running, open Swagger in your browser to explore and test the APIs:

```
http://localhost:<port>/swagger
```

### Step 5 — Run Everything with Docker Compose

To start MySQL, Redis, RabbitMQ, the main API, the background worker, and the internal directory service all together:

```bash
docker-compose up --build
```

Services communicate using container names, not localhost. Credentials for all services must be set in environment configuration, not committed to source files.

---

## How Authentication Works

Most APIs require a valid JWT token. To get one, call the login endpoint first.

**Login:**
```
POST /api/auth/login
```

Request body:
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:
```json
{
  "token": "jwt-token-value",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "admin",
    "role": "Admin"
  }
}
```

For every protected API call, add this header:
```
Authorization: Bearer <token>
```

The only public endpoints that do not need a token are `GET /api/health` and `POST /api/auth/login`.

**Test credentials:**
- Username: `admin`
- Password: `Admin@123`

---

## How File Upload Works

To upload a submission file:

1. Authenticate and get a JWT token
2. Call `POST /api/submissions/{submissionId}/files` with `multipart/form-data`
3. The API validates the file, saves metadata to MySQL, and publishes a message to RabbitMQ
4. You receive `202 Accepted` with a tracking ID
5. The background worker picks up the message, processes the message, and updates the job status
6. Poll `GET /api/processing-jobs/{id}` to check if processing is complete
7. Download the file with `GET /api/submission-files/{id}/download`

File security rules:
- Empty files and files above the configured size limit are rejected
- Only allowed file extensions are accepted
- Physical file names are always server-generated the original file name is never used on disk
- The storage path is never exposed in API responses

---

## Caching

Redis is used as a distributed cache for frequently read data like trainee profiles, task assignment and submission.

- On a cache miss the API reads from MySQL, stores the result in Redis with a TTL, and returns it
- Cache keys follow the pattern `trainee:{id}`, `submission:{id}`
- The cache is invalidated whenever a record is created, updated, or deleted
- If Redis is unavailable the API falls back to MySQL it does not fail

---

## Asynchronous Processing

Submission file processing is handled asynchronously through RabbitMQ and a separate worker service.

- Queue name: `submission-processing` (durable, persistent)
- The API publishes a message after a valid file upload and returns immediately.
- The worker consumes one message at a time, update the task assignment status, also make if any one have same file so this filename replace by that filename and delete that for storage optimization, and acknowledges only after success
- If processing fails after retries, the message is moved to a dead-letter queue and the job is marked as Failed
- The worker is idempotent duplicate messages are detected and skipped safely

---

## Internal Service Communication

`TrainingDirectory.Api` is a small internal service that returns data for trainee when request is come for readonly.

- Communication uses `HttpClient` with a configured base address and timeout
- A correlation ID is passed through every API call, database record, RabbitMQ message, and worker log so the full lifecycle of any request can be traced in logs

---

## Health Checks

```
GET /health/live    → liveness check
GET /health/ready   → readiness check (MySQL, Redis, RabbitMQ, internal service)
```

---

## Security Practices

- Passwords are always stored as hashes plain text passwords are never stored or logged
- `PasswordHash` is never returned in any API response
- JWT signing key is read from configuration, not hardcoded
- DTOs are used to control what data is exposed entities are never returned directly
- All data access goes through EF Core no raw SQL queries
- Global exception middleware catches unexpected errors and returns a safe message without stack traces
- Logs never contain passwords, JWT tokens, connection strings, or file contents
- CORS is restricted to `http://localhost:3000` and `http://localhost:5173` for local development

---

## Known Limitations

- Local disk is used for file storage, not cloud storage.
- MySQL, Redis, and RabbitMQ run as single-node no clustering or high availability
- No email notifications or real-time updates

---

## Next Improvement Areas

- Connect a React frontend
- Replace local file storage with cloud object storage
- Add API versioning
- Add role-based access control per endpoint