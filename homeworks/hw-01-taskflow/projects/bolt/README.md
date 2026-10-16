# TaskFlow REST API

A small REST API for managing tasks, built with **.NET 10**, **ASP.NET Core Web API**, and **xUnit**.

## Features

- Create, list, and retrieve tasks
- Update task status (New, InProgress, Completed)
- Thread-safe in-memory storage (no database required)
- Input validation with meaningful error messages
- Swagger UI for interactive testing
- xUnit integration tests

## Project Structure

```
TaskFlow.slnx
TaskFlow.Api/
  Models/
    TaskItem.cs          # Task entity
    TaskStatus.cs        # Status enum
    Dtos.cs              # Request/response DTOs + error envelope
  Services/
    ITaskRepository.cs   # Repository interface
    InMemoryTaskRepository.cs  # Thread-safe ConcurrentDictionary storage
    ITaskService.cs      # Service interface
    TaskService.cs       # Business logic + validation
  Controllers/
    TasksController.cs   # REST endpoints
  Program.cs             # DI setup, Swagger, pipeline
TaskFlow.Tests/
  TaskFlowApiTests.cs    # Integration tests (3 minimum tests)
```

## Architecture

The application follows a clean **three-layer** design:

1. **Models** — Data structures: `TaskItem`, `TaskItemStatus`, and DTOs for request/response payloads.
2. **Services** — Business logic layer. `TaskService` handles validation (title required, max 100 chars) and delegates persistence to `ITaskRepository`. `InMemoryTaskRepository` uses `ConcurrentDictionary` for thread-safe in-memory storage.
3. **Controllers** — Thin HTTP layer that maps service results to proper HTTP status codes (200, 201, 400, 404).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## How to Run

### 1. Restore dependencies

```bash
dotnet restore
```

### 2. Build the solution

```bash
dotnet build
```

### 3. Run tests

```bash
dotnet test
```

### 4. Run the API

```bash
dotnet run --project TaskFlow.Api
```

The API will be available at `http://localhost:5000` (or the port shown in the console).

### 5. Swagger UI

Once the app is running, open the browser and navigate to:

```
http://localhost:<port>/swagger
```

The Swagger UI lets you try every endpoint interactively — create tasks, list them, fetch by id, and change status.

## API Endpoints

| Method | Route | Description | Success | Errors |
|-------|-------|-------------|---------|--------|
| `GET` | `/api/tasks` | Get all tasks | 200 | — |
| `GET` | `/api/tasks/{id}` | Get task by ID | 200 | 404 |
| `POST` | `/api/tasks` | Create a task | 201 | 400 |
| `PATCH` | `/api/tasks/{id}/status` | Change task status | 200 | 400, 404 |

### Examples

**Create a task:**

```bash
curl -X POST http://localhost:5000/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"My first task","description":"Test the API"}'
```

**Change status:**

```bash
curl -X PATCH http://localhost:5000/api/tasks/<GUID>/status \
  -H "Content-Type: application/json" \
  -d '{"status":1}'
```

Status values: `0` = New, `1` = InProgress, `2` = Completed.

## Validation Rules

- **Title** is required and must not exceed 100 characters.
- **Status** must be a valid enum value (0, 1, or 2).
- Missing tasks return `404 Not Found`.

## Tests

Three xUnit integration tests cover the core flows:

1. **CreateTask_ValidTitle_ReturnsCreated** — Creates a task with a valid title and checks the 201 response.
2. **CreateTask_EmptyTitle_ReturnsBadRequest** — Verifies that an empty title is rejected with 400.
3. **UpdateStatus_ExistingTask_ReturnsOkWithUpdatedStatus** — Creates a task, updates its status, and verifies the change.
