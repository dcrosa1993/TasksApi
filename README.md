# Tasks API

REST API for task management, developed with ASP.NET Core and C#. It supports creating, retrieving, updating, and deleting tasks, as well as filtering tasks by status and responsible person with pagination.

## Technologies

- .NET 10 / ASP.NET Core Web API
- C#
- Entity Framework Core 10
- SQLite
- EF Core Migrations
- Swagger / OpenAPI (Swashbuckle)
- xUnit
- WebApplicationFactory for integration testing

## Requirements

- .NET SDK 10.0.102 or a compatible version specified in `global.json`.
- Git.

No database server installation is required. SQLite runs locally.

## Project Structure

```text
TasksApi/
├── src/
│   └── TasksApi.Api/
│       ├── Controllers/
│       ├── Domain/
│       │   ├── Entities/
│       │   └── Enums/
│       ├── DTOs/
│       │   └── Tasks/
│       ├── Infrastructure/
│       │   └── Persistence/
│       │       └── Migrations/
│       ├── Program.cs
│       └── appsettings.json
├── tests/
│   └── TasksApi.Tests/
│       ├── Controllers/
│       └── Infrastructure/
├── global.json
├── dotnet-tools.json
└── TasksApi.slnx
```

## Installation and Execution

Clone the repository:

```bash
git clone https://github.com/dcrosa1993/TasksApi
cd TasksApi
```

Restore local tools and dependencies:

```bash
dotnet tool restore
dotnet restore
```

Create and update the database by applying migrations:

```bash
dotnet tool run dotnet-ef database update \
  --project src/TasksApi.Api \
  --startup-project src/TasksApi.Api
```

Run the API:

```bash
dotnet run --project src/TasksApi.Api
```

The application uses SQLite and creates the `tasks.db` file inside the API project directory.

## Swagger UI

With the application running in the Development environment, access:

```text
http://localhost:5118/swagger
```

The port may vary depending on the launch configuration. The actual address is displayed in the console when the application starts.

Swagger provides interactive API documentation and allows users to execute all API operations directly from the browser.

## Data Model

Each task contains the following properties:

| Field | Type | Description |
|---|---|---|
| Id | Guid | Unique identifier |
| Title | string | Task title |
| Description | string? | Optional description |
| Status | TaskState | Task status |
| Responsible | string | Person responsible |
| DueDate | DateTimeOffset | Task deadline |

Available statuses:

- `Pending`: Pending.
- `InProgress`: In progress.
- `Done`: Completed.

## API Endpoints

### Create a task

```http
POST /api/tasks
```

Example:

```json
{
  "title": "Implement REST API",
  "description": "Develop task management endpoints",
  "status": "InProgress",
  "responsible": "User",
  "dueDate": "2026-11-20T18:00:00-04:00"
}
```

The status field is optional when creating a task. If omitted, it defaults to `Pending`.

Response: `201 Created`.

### List tasks

```http
GET /api/tasks
```

Supports filtering and pagination:

| Parameter | Type | Description |
|---|---|---|
| status | TaskState? | Filter by status |
| responsible | string? | Filter by responsible person |
| page | int | Page number (default: 1) |
| pageSize | int | Number of results per page (default: 10, maximum: 100) |

Example:

```http
GET /api/tasks?status=InProgress&responsible=Denismay&page=1&pageSize=10
```

The response includes:

- `items`: Tasks on the current page.
- `page`: Current page number.
- `pageSize`: Page size.
- `totalCount`: Total number of filtered results.
- `totalPages`: Total number of available pages.

### Retrieve a task

```http
GET /api/tasks/{id}
```

Response: `200 OK` or `404 Not Found`.

### Update a task

```http
PUT /api/tasks/{id}
```

All required resource fields must be provided.

Response: `204 No Content` or `404 Not Found`.

### Delete a task

```http
DELETE /api/tasks/{id}
```

Response: `204 No Content` or `404 Not Found`.

## Validation and HTTP Error Handling

The API uses Data Annotations and automatic validation through `[ApiController]`.

| Status Code | Description |
|---|---|
| 200 | Request completed successfully |
| 201 | Resource created |
| 204 | Update or deletion completed successfully |
| 400 | Invalid data or query parameters |
| 404 | Resource not found |
| 500 | Internal server error |

Validation errors use `ValidationProblemDetails`, while missing resources return `ProblemDetails`.

## Persistence and Technical Decisions

**SQLite**

SQLite was chosen to simplify setup and eliminate external infrastructure dependencies. The application can run without configuring a database server.

**Entity Framework Core**

EF Core is used for data access and database schema versioning through migrations.

Indexes have been created on `Status` and `Responsible` to support filtered queries.

**DTOs**

Domain entities are not exposed directly through the API. Separate DTOs are used for request and response contracts.

**Queries**

Read operations use `AsNoTracking`, and database operations are executed asynchronously.

Filtering, counting, and pagination are performed at the database level.

**Pagination**

The page size is limited to a maximum of 100 records to prevent excessively large queries.

**Responsible Person**

The responsible person is represented as a string field because user management and authentication are outside the requested scope.

**Ordering**

The task listing uses stable ordering by identifier. Configurable ordering by deadline is not currently supported.

## Testing

The test project uses xUnit, `WebApplicationFactory`, and SQLite in-memory databases.

Tests execute HTTP requests against the application and verify both API responses and persistence behavior.

Run all tests:

```bash
dotnet test
```

Current test coverage includes:

- Task creation and default status.
- Retrieving existing tasks.
- Updating tasks.
- Deleting tasks.
- Input validation.
- Retrieving non-existent resources.
- Combined filtering by status and responsible person.
- Pagination.
- Pagination parameter validation.

Tests use an isolated database instance and apply the project's actual migrations.

## Future Improvements

Due to time and scope constraints, development prioritized the explicitly requested functionality.

Given more time, I would consider:

1. Configurable ordering by deadline, using a SQLite-compatible conversion for `DateTimeOffset`.
2. Additional test coverage for edge cases and input validation.
3. Concurrency control to prevent simultaneous modifications to the same task.
4. Optional authentication and authorization if the API is integrated into a multi-user environment.
5. Further improvements to observability and centralized exception handling.

## AI Usage

ChatGPT was used as a development support tool throughout the implementation.

**Tasks where AI was used:**

- Initial solution design and project structure.
- Reviewing domain model and persistence decisions.
- Generating and reviewing DTOs and CRUD endpoints.
- Diagnosing compatibility issues between Entity Framework Core and SQLite.
- Designing integration tests.
- Reviewing technical decisions and documentation.

**Accepted suggestions:**

- Separation between domain entities and DTOs.
- Use of EF Core and SQLite with migrations.
- Use of controllers and automatic validation.
- Integration testing with WebApplicationFactory.

**Corrections made:**

During implementation, an issue was identified when ordering `DateTimeOffset` values through SQLite. The initial implementation directly ordered by the deadline, but the EF Core provider did not support that operation.

The issue was investigated, and stable ordering by identifier was adopted instead, keeping queries and pagination at the database level.

AI-generated suggestions and code were executed and verified through builds, automated tests, and manual testing using Swagger.

AI was used as a development, troubleshooting, and technical review assistant. Final decisions and behavioral validation were performed during implementation.

---

Project developed as an ASP.NET Core Web API technical assessment.