# 🎓 Upskeel LMS Backend

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET 9](https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=for-the-badge&logo=postgresql&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens&logoColor=white)
![React](https://img.shields.io/badge/React-61DAFB?style=for-the-badge&logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)

---

## 📋 Overview

**Upskeel** is a **Learning Management System (LMS)**. It provides a secure, scalable platform for course creation, student enrollment, and instructor approval workflows with role-based access control.

![Upskeel Dashboard](./assets/dashboard.png)

---

## 🌐 Live URLs

| Environment | URL |
|---|---|
| **Frontend** | https://upskeel.vercel.app/login |
| **Backend API** | https://upskeel.up.railway.app |

---

## 🚀 Features

### 👩‍🏫 Instructor Features
- Request instructor status with admin approval workflow
- Create, edit, and manage courses
- View enrolled students and track engagement
- Manage course content and lessons

### 🎓 Student Features
- Browse and search available courses
- Enroll in courses
- Track learning progress
- View course materials and lessons

### 🛠️ Admin Features
- User and role management
- Approve/deny instructor requests
- Create and manage courses
- Full CRUD operations on users and courses
- System monitoring and analytics

---

## 🧱 Tech Stack

### Backend
| Layer | Technology |
|-------|-------------|
| **Framework** | ASP.NET Core 9 (.NET 9) |
| **Language** | C# 12 |
| **ORM** | Entity Framework Core |
| **Database** | PostgreSQL |
| **Authentication** | JWT Bearer Tokens |
| **Authorization** | Identity Roles (Admin, Instructor, Student) |
| **Testing** | xUnit + WebApplicationFactory |
- **API Documentation:** Swagger/OpenAPI


### Frontend
| Layer | Technology |
|-------|-------------|
| **Framework** | React 18 |
| **Language** | TypeScript |
| **Build Tool** | Vite |

---

## 📁 Project Structure

### Backend
```
/backend
  ├── Controllers
  │   ├── AuthController.cs
  │   ├── CoursesController.cs
  │   └── EnrollmentController.cs
  │   └── InstructorRequestsController.cs
  ├── Data
  │   ├── ApplicationDbContext.cs
  │   └── DbInitializer.cs
  ├── Migrations
  ├── Models
  │   ├── DTOs
  │   └── Enums
  │   ├── ApplicationUser.cs
  │   ├── Course.cs
  │   └── Enrollment.cs
  │   └── InstructorAppprovalRequest.cs
  ├── Properties
  ├── Services
  │   ├── EmailService.cs
  │   └── TokenService.cs
  └── Program.cs
```

### Frontend
```
/frontend
  ├── public
  ├── src
  │   ├── assets
  │   ├── components
  │   ├── hooks
  │   ├── pages
  │   ├── services
  │   ├── store
  │   └── App.tsx
  └── index.html
```

---

## ⚙️ Setup

### Prerequisites
- .NET 9 SDK
- PostgreSQL 12+
- Git

### Backend Setup

1. **Clone the repository:**

2. **Create `appsettings.json` in the root directory:**

3. **Install dependencies:**
```bash
dotnet restore
```

4. **Update the database connection string:**
```json
// appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=your_db_name;Username=your_username;Password=your_password"
}
```

5. **Start the backend:**
```bash
dotnet run
```

The API will be available at:
- http://localhost:8080
- https://localhost:8081
- Swagger UI: https://localhost:8081/swagger

### Frontend Setup

Refer to the [frontend repository](https://github.com/chumavii/lms-frontend) for React + TypeScript setup instructions.

---

## 🔐 Authentication & Authorization

### JWT Token Flow
1. User registers via `/api/auth/register`
2. User logs in via `/api/auth/login`
3. Backend returns JWT token
4. Client includes token in `Authorization: Bearer <token>` header
5. Server validates token and grants access based on roles

### Available Roles
- **Admin** - Full system access
- **Instructor** - Create and manage courses (after approval)
- **Student** - Enroll in courses

---

## 🗄️ Database Schema

### Core Entities
- **ApplicationUser** - User accounts with identity
- **Course** - Course information and metadata
- **Enrollment** - Student-Course relationships
- **Lesson** - Course lesson content
- **InstructorApprovalRequest** - Instructor approval workflow

### Relationships

---

## 📄 API Documentation

OpenAPI documentation is available at `/swagger` endpoint after starting the backend server.

---

## License

This project is open source and available under the MIT License.

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## Author

**Chuma**  
Backend Engineer • Automation Developer • Cloud Enthusiast  
[GitHub @chumavii](https://github.com/chumavii)