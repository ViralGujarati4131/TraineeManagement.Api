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
├── Controllers/
│   ├── HealthController.cs
│   ├── TraineesController.cs
│   ├── UserController.cs
│   ├── MentorsController.cs
│   ├── LearningTasksController.cs
├── Models/
│   ├── Trainee.cs
│   ├── User.cs
│   ├── Mentor.cs
│   ├── LearningTask.cs
├── DTOs/
│   ├── TraineeDto
│   ├── UserDto
│   ├── MentorDto
│   ├── LearningTaskDto
├── Interfaces/
│   ├── ILearningTaskServices
│   ├── IMentorServices
│   ├── ITimeStamp
│   ├── ITraineeServices
│   ├── IUserServices
├── Services/
│   ├── ITraineeServices.cs
│   ├── IUserServices.cs
│   ├── IMentorServices.cs
│   ├── ILearningTaskServices.cs
├── Utils/
│   ├── CustomException.cs
│   ├── JwtService.cs
│   ├── UserSeeder.cs
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
- [MySQL Server 8.x](https://dev.mysql.com/downloads/mysql/)I

### 1. Clone the Repository

```bash
git clone <https://github.com/ViralGujarati4131/TraineeManagement.Api>
cd TraineeManagement.Api
```

### 2. Restore NuGet Packages Clean Project And Build It

```bash
dotnet restore
dotnet clean
dotnet build
```

---

## MySQL Setup Steps

### 1. Create the Database

Log in to MySQL and run:

```sql
CREATE DATABASE trainee_management_db;
```

### 2. Configure Connection String

Update `appsettings.json` with your MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=trainee_management_db;user=root;password=your_password;"
  }
}
```

### 3. Start Mysql

```bash
dotnet service mysql start
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

> Seed an Admin user during application startup.

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

## API List

### Public APIs (No Token Required)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/health` | Health check |
| POST | `/api/auth/login` | Login and get JWT token |

### Trainee APIs (Protected)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/trainees?search` | Get all trainees (Name|Email|Techstack) |
| GET | `/api/trainees/paginationSearch?pageNumber=1&pageSize=10&search=amit&status=Active` | Get all trainees (paginated) |
| GET | `/api/trainees/{id}` | Get trainee by ID |
| POST | `/api/trainees` | Create a new trainee |
| PUT | `/api/trainees/{id}` | Update trainee details |
| DELETE | `/api/trainees/{id}` | Delete a trainee |

### Mentor APIs (Protected)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/mentors` | Get all mentors |
| GET | `/api/mentors/{id}` | Get mentor by ID |
| POST | `/api/mentors` | Create a new mentor |
| PUT | `/api/mentors/{id}` | Update mentor details |
| DELETE | `/api/mentors/{id}` | Delete a mentor |

### Learning Task APIs (Protected)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/learning-tasks` | Get all learning tasks |
| GET | `/api/learning-tasks/{id}` | Get learning task by ID |
| POST | `/api/learning-tasks` | Create a new learning task |
| PUT | `/api/learning-tasks/{id}` | Update a learning task |
| DELETE | `/api/learning-tasks/{id}` | Delete a learning task |

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
| Password storage | Passwords stored as hash only — plain text never stored or logged |
| Excessive data exposure | DTOs used for all responses; `PasswordHash` never returned |
| Injection prevention | EF Core parameterized queries used; no raw unsafe SQL |
| Security misconfiguration | CORS restricted to known React dev origins |
| Logging | Passwords, tokens, and sensitive data never logged |

---

## Logging

The following events are logged:

- User login success and failure
- Trainee created, updated, and deleted
- Mentor created, updated, and deleted
- Record-not-found (404) cases
- Unexpected exceptions

---

## Known Limitations

---

## Next Improvement Areas

