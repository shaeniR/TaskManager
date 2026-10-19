# TaskFlow — Task & Project Management System

A full-stack mini Trello/Jira built with **ASP.NET Core 8 Web API**, **React (Vite)** and **MySQL**.

## Features
- 🔐 Register / Login with **JWT authentication** and **BCrypt** password hashing
- 👥 **Role-based authorization** (Admin / Member) + project-level permissions (owner / member)
- 📁 Projects: create, edit, delete, add / remove members
- ✅ Tasks: create, edit, delete, assign, priority, due date, comments
- 🗂️ **Kanban board with drag & drop** (To Do → In Progress → Done)
- 🔍 Search, filter (priority, assignee, status) and **server-side pagination** (list view)
- 📊 Dashboard: task counts by status, completion %, overdue tasks, my upcoming tasks
- 🛡️ FluentValidation, global exception middleware, Swagger with JWT support

## Tech Stack
| Layer    | Technology |
|----------|------------|
| Frontend | React 18, Vite, React Router, Axios, react-hot-toast |
| Backend  | ASP.NET Core 8 Web API, Entity Framework Core 8, FluentValidation |
| Database | MySQL 8 (Pomelo EF Core provider) |
| Auth     | JWT Bearer tokens, BCrypt |

## Architecture
```
React (5173) ──HTTP + JWT──▶ ASP.NET Core API (5000) ──EF Core──▶ MySQL (3306)

Controller  →  Service (business logic)  →  AppDbContext (EF Core)  →  MySQL
```

```
backend/TaskManager.API/
├── Controllers/   API endpoints (thin — no business logic)
├── Services/      Business logic + permission checks
├── Models/        EF Core entities
├── DTOs/          Request/response shapes (entities are never exposed)
├── Validators/    FluentValidation rules
├── Middleware/    Global exception handling
├── Data/          DbContext + seed data
└── Migrations/    EF Core migrations

frontend/src/
├── api/           Axios instance (JWT interceptor) + API functions
├── context/       AuthContext (logged-in user)
├── components/    Layout, ProtectedRoute, TaskCard, modals
└── pages/         Login, Register, Dashboard, Projects, ProjectBoard
```

## Database (ER)
```
Users ──< ProjectMembers >── Projects ──< Tasks ──< Comments
  └───────────── AssignedTo ──────────────┘
```
- `ProjectMembers` is the join table for the **many-to-many** User ↔ Project relationship.
- Deleting a project cascades to its tasks and comments; deleting a user sets `AssignedToId` to NULL.

## Running locally

### Prerequisites
.NET 8 SDK, Node.js 18+, MySQL 8

### 1. Backend
1. Open `backend/TaskManager.API/appsettings.json` and set your MySQL password in `ConnectionStrings:DefaultConnection`.
2. Run:
   ```bash
   cd backend/TaskManager.API
   dotnet run
   ```
   The database `taskmanager_db` is created automatically (migrations + demo data run on startup).
   Swagger: http://localhost:5000/swagger

### 2. Frontend
```bash
cd frontend
npm install
npm run dev
```
Open http://localhost:5173

### Demo accounts
| Role   | Email                   | Password   |
|--------|-------------------------|------------|
| Admin  | admin@taskmanager.com   | Admin@123  |
| Member | john@taskmanager.com    | John@123   |
| Member | nimali@taskmanager.com  | Nimali@123 |

## API Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST   | /api/auth/register | Create account |
| POST   | /api/auth/login | Login → JWT |
| GET    | /api/auth/me | Current user |
| GET    | /api/dashboard | Dashboard stats |
| GET    | /api/projects | My projects |
| POST   | /api/projects | Create project |
| GET / PUT / DELETE | /api/projects/{id} | Project details / update / delete (owner) |
| POST   | /api/projects/{id}/members | Add member (owner) |
| DELETE | /api/projects/{id}/members/{userId} | Remove member (owner) |
| GET    | /api/projects/{id}/tasks?status=&priority=&assignedToId=&search=&page=&pageSize= | Filtered, paginated tasks |
| POST   | /api/tasks | Create task |
| PUT    | /api/tasks/{id} | Update task |
| PATCH  | /api/tasks/{id}/status | Change status (drag & drop) |
| DELETE | /api/tasks/{id} | Delete task (owner / admin) |
| GET / POST | /api/tasks/{id}/comments | List / add comments |
| GET    | /api/users?search= | Search users |
| PUT    | /api/users/{id}/role | Change role (**Admin only**) |

## Future improvements
- Real-time updates with SignalR
- Unit & integration tests (xUnit)
- Refresh tokens
- Docker Compose for one-command setup
- File attachments on tasks
