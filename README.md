# RaceDay API - PROG6212 Part 2

A RESTful ASP.NET Core Web API for the **RaceDay** event management system. This API powers the entire RaceDay platform, handling events, categories, enrolments, results, and user authentication with role-based access control.

---

## CI/CD Status Screenshot

Screenshot 2026-10-16 171828.png

---

## Roles

The system supports two distinct user roles, each with specific permissions:

### Organiser
- Create, update, and delete race events
- Define reusable race categories (e.g., 5km, 10km, Half Marathon)
- Assign categories to specific events
- View all participant enrolments for their events
- Capture finish times and finishing positions for participants

### Participant
- Register and login to the system
- View and update their own profile
- Browse available events and categories
- Enrol in an event by selecting a category
- View and cancel their own enrolments
- View their own race results and history

---

## Technologies Used

- **ASP.NET Core 8 Web API** — Backend framework
- **Entity Framework Core (Code-First)** — ORM for SQL Server
- **SQL Server** — Database (matches Part 1 ERD and SQL script)
- **JWT (JSON Web Tokens)** — Session management and authentication
- **BCrypt.Net** — Secure password hashing
- **Swagger / Swashbuckle** — Interactive API documentation
- **xUnit** — Unit testing framework
- **GitHub Actions** — Automated CI/CD pipeline

---

## Setup Instructions

Follow these steps to run the API locally:

### 1. Clone the Repository
```bash
git clone https://github.com/ST10489247OnalennaMokoetsi/PROG6212RaceDayPart2.git
2. Open in Visual Studio
Open the solution file RaceDaySystem.sln in Visual Studio 2022.

3. Configure the Database Connection
Open RaceDay.API/appsettings.json and update the connection string to match your local SQL Server instance:

json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
Replace YOUR_SERVER_NAME with your SQL Server instance name (e.g., BLACK-ASUS2054\\SQLEXPRESS).

4. Create the Database
Open the Package Manager Console (Tools > NuGet Package Manager > Package Manager Console), make sure the default project is set to RaceDay.API, and run:

powershell
Update-Database
This will create the RaceDayDB database and all tables based on the Entity Framework Core migrations.

5. Run the API
Press F5 to run the project. Your browser will open the Swagger UI where you can interact with all API endpoints.

API Documentation (Swagger)
The API is fully documented with Swagger. All endpoints are visible, testable, and include clear descriptions of their purpose, expected input, and expected responses.

To test authenticated endpoints:

Register a new user via POST /api/auth/register.

Login via POST /api/auth/login to receive a JWT token.

Click the Authorize button at the top of the Swagger UI.

Enter Bearer <your-token> and click Authorize.

You can now call protected endpoints such as GET /api/users/me.

Unit Tests
This project includes 9 comprehensive unit tests covering:

✅ User registration (success and duplicate email failure)

✅ User login (valid credentials and invalid password)

✅ Role-based access enforcement (403 Forbidden for unauthorised roles)

✅ Event enrolment logic (valid enrolment, closed registration, duplicate enrolment)

Running the Tests
In Visual Studio, open Test > Test Explorer and click Run All Tests. All tests should pass with green checkmarks.

The same tests are automatically executed via GitHub Actions on every push, as shown by the green build badge at the top of this README.

YouTube Video Link:
https://youtu.be/YAxtmyu4xdM

Project Structure
text
RaceDaySystem/
├── .github/
│   └── workflows/
│       └── dotnet.yml             # GitHub Actions CI/CD pipeline
├── RaceDay.API/
│   ├── Controllers/               # API endpoint controllers
│   ├── Data/                      # DbContext and EF Core configuration
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Migrations/                # EF Core Code-First migrations
│   ├── Models/                    # Entity Framework database models
│   ├── appsettings.json           # Configuration (connection string, JWT)
│   └── Program.cs                 # Application entry point
├── RaceDay.Tests/
│   └── ApiTests.cs                # xUnit unit tests
└── README.md

Onalenna Mokoetsi — ST10489247

Module: PROG6212 — Programming 2B
