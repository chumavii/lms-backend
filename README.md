# 🎓 Upskeel LMS Backend

**Upskeel** is a full-featured **Learning Management System (LMS)** designed to streamline online education for students, instructors, and administrators.
Built with **ASP.NET Core** and **React + TypeScript**, it provides a scalable, role-based platform for course creation, enrollment, and instructor approval management.

## 🌐 Live URLs
- **Frontend:** https://upskeel.vercel.app/login
- **Backend API:** https://upskeel.up.railway.app

## 🚀 Features
### 👩‍🏫 Instructors
- Request instructor status and await admin approval.
- Create and manage courses once approved.
- View and manage student enrollments.

### 🎓 Students
- Browse, search, and enroll in available courses.
- Track learning progress and view enrolled courses.

### 🛠️ Admins
- Manage users, roles, and account statuses.
- Approve or deny instructor requests.
- View, update, or delete any course or user account.

## 🧱 Tech Stack
### Backend
- ASP.NET Core 9 (C#)
- Entity Framework Core
- PostgreSQL (Railway deployment)
- JWT Authentication & Identity Roles

### Frontend
- React + TypeScript
- Vite
- TailwindCSS

## ⚙️ Setup
### Backend
Create `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=lmsdb;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "Upskeel",
    "Audience": "UpskeelUsers"
  }
}
```

Run migrations:
```
dotnet ef database update
```

Start:
```
dotnet run
```

## 👨‍💻 Author
**Chuma**  
