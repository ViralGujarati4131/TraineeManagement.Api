# TraineeManagement.Api

## Project Overview

TraineeManagement.Api is an ASP.NET Core Web API developed for managing trainee records.

The project demonstrates:

- ASP.NET Core Web API
- Controllers
- DTOs
- Service Layer
- EF Core InMemory Database
- CRUD Operations
- Validation
- Swagger



## Technology Stack

 C#
 .NET 9 
 ASP.NET Core Web API
 Entity Framework Core
 EF Core InMemory Database
 Swagger


## Project Structure

TraineeManagement.Api

─ Controllers
     ─ HealthController.cs
     ─ TraineeController.cs

─ Models
     ─ Trainee.cs

─ DTOs
     ─ TraineeDto.cs

─ Services
     ─ Interfaces
         ─ ITraineeServices.cs
    
     ─ TraineeServices.cs

─ Data
     ─ DbContext.cs

─ Program.cs



## How to Run
 
### Clone the repo first
https://github.com/ViralGujarati4131/TraineeManagement.Api.git
### Navigate to Project
cd TraineeManagement.Api
### Restore Packages
dotnet restore
### Run Application
dotnet run

### Open Swagger
https://localhost:<port>/swagger


## API Endpoints

### Health Check
GET  /api/health
- Sample Get Health Response
{
  "status": "running",
  "application": "Trainee Management API",
  "timestamp": "2026-06-08T13:13:44.7536409+00:00"
}



### Trainee APIs


GET  /api/trainees 
- Sample GET Response
- 
```json
[
  {
    "id": 1,
    "firstName": "Amit",
    "lastName": "Sharma"
  }
]
```



GET  /api/trainees/{id} 
- id = 1
- Sample GetById Response
[
  {
    "id": 1,
    "firstName": "Amit",
    "lastName": "Sharma"
  }
]



POST  /api/trainees 
- {
  "firstName": "Amit",
  "lastName": "Sharma",
  "email": "amit.sharma@training.com",
  "techStack": "HTML, CSS, JavaScript",
  "status": "Active"
}
-  Sample Success Response
{
  "id": 1,
  "firstName": "Amit",
  "lastName": "Sharma"
}



PUT  /api/trainees/{id} 
- id = 1
-  {
  "firstName": "Viral",
  "lastName": "Gujarati",
  "email": "viral.gujarati@yahoo.com",
  "techStack": "React Native, Node",
  "status": "Active"
}
- Sample Success Response
{
  "id": 1,
  "firstName": "Viral",
  "lastName": "Gujarati"
}



DELETE  /api/trainees/{id} 
- id = 1
 - Smaple Success Response
 204 NoContent



GET  /api/trainees?search=value 
- search = viral
 - Sample Success Response
{
  "id": 1,
  "firstName": "Viral",
  "lastName": "Gujarati"
}


## limitations

- data is stored is in InMemoryStorage so it clear when server restart.

## Next Phase Scope

- We can configure database so data is persistent.
- We can create more endpoints to handle media files.