# JiraLite - Issue Tracking System

JiraLite is a lightweight, modern issue tracking and project management system built with ASP.NET Core 8.0 and MySQL. It provides essential features for managing tasks, bugs, and workflows in software development teams.

## 📋 Table of Contents

- [Features](#features)
- [Demo Video](#demo-video)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## ✨ Features

### Core Features
- 🔐 **User Authentication & Authorization**
  - JWT-based authentication for API endpoints
  - Session-based authentication for MVC views
  - Role-based access control (Admin/User)
  - Secure password hashing with BCrypt

- 📝 **Issue Management**
  - Create, read, update, and delete issues
  - Issue types: Bug, Task
  - Priority levels: High, Medium, Low
  - Status tracking: Open, In Progress, Closed, Reopened
  - Rich text descriptions
  - Issue comments and history tracking

- 📊 **Workflow Management**
  - Customizable workflow transitions
  - Admin-only workflow configuration
  - Status transition validation
  - Automatic history tracking for status changes

- 💬 **Comments System**
  - Add comments to issues
  - View comment history with timestamps
  - User attribution for comments

- 📥 **Bulk Upload**
  - Excel file import for bulk issue creation
  - Support for .xlsx and .xls formats
  - Validation and error reporting
  - Success/failure tracking for batch operations

- 📈 **Dashboard & Analytics**
  - View all issues in a centralized dashboard
  - Filter and search capabilities
  - Quick issue status overview
  - User-specific issue views

### Technical Features
- RESTful API endpoints with Swagger documentation
- MVC architecture with Razor views
- Entity Framework Core with MySQL
- Clean architecture with separation of concerns
- Session management
- CORS support for frontend integration

## 🎥 Demo Video


> [Watch Demo Video](https://drive.google.com/file/d/1QlTEUhxVjkR8-zacPqtifLcS_gX_wrlY/view?usp=sharing)
> 


## 🛠 Tech Stack

### Backend
- **Framework:** ASP.NET Core 8.0
- **Language:** C# 12
- **Database:** MySQL 8.0
- **ORM:** Entity Framework Core 8.0
- **Authentication:** JWT Bearer + Session-based

### Frontend
- **View Engine:** Razor Pages
- **CSS Framework:** Bootstrap 5
- **JavaScript:** jQuery
- **Icons:** Bootstrap Icons

### Key Libraries & Packages
- `Pomelo.EntityFrameworkCore.MySql` - MySQL database provider
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `BCrypt.Net-Next` - Password hashing
- `EPPlus` - Excel file processing
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [JetBrains Rider](https://www.jetbrains.com/rider/) / [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/jiralite.git
cd jiralite
```

### 2. Restore Dependencies

```bash
cd JiraLite
dotnet restore
```

## ⚙️ Configuration

### 1. Update Connection String

Edit `appsettings.json` and update the MySQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=JiraLite;User=your_username;Password=your_password;"
  }
}
```

### 2. Configure JWT Settings

Update JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWT2026MinimumLength32Characters!",
    "Issuer": "JiraLite",
    "Audience": "JiraLiteUsers",
    "ExpiryMinutes": "1440"
  }
}
```

> ⚠️ **Important:** Change the `SecretKey` to a strong, unique value in production!

## 🗄️ Database Setup

### 1. Create Database

```sql
CREATE DATABASE JiraLite;
```

### 2. Run Migrations

```bash
cd JiraLite
dotnet ef database update
```

This will create all necessary tables:
- Users
- Issues
- IssueComments
- IssueHistories
- WorkflowTransitions

### 3. Seed Initial Data (Optional)

You can manually create an admin user through the registration page, or insert directly into the database.

## ▶️ Running the Application

### Development Mode

```bash
cd JiraLite
dotnet run
```

Or with hot reload:

```bash
dotnet watch run
```

The application will start on:
- **HTTPS:** https://localhost:7001
- **HTTP:** http://localhost:5001

### Access Points

- **Web UI:** https://localhost:7001
- **Swagger API Docs:** https://localhost:7001/swagger
- **API Base URL:** https://localhost:7001/api

## 📚 API Documentation

### Authentication Endpoints

```
POST   /api/auth/register          - Register new user
POST   /api/auth/login             - Login user
```

### Issue Endpoints

```
GET    /api/issues                 - Get all issues
GET    /api/issues/my              - Get current user's issues
GET    /api/issues/{id}            - Get issue by ID
POST   /api/issues                 - Create new issue
PUT    /api/issues/{id}            - Update issue
POST   /api/issues/upload-excel    - Bulk upload from Excel
```

### Comment Endpoints

```
GET    /api/issues/{id}/comments         - Get issue comments
POST   /api/issues/{id}/comments         - Add comment to issue
```

### History Endpoints

```
GET    /api/issues/{id}/history          - Get issue status history
```

### Workflow Endpoints

```
GET    /api/issues/{id}/allowed-transitions    - Get allowed status transitions
POST   /api/issues/{id}/transition            - Transition issue status
```

For detailed API documentation, visit `/swagger` after running the application.

## 📁 Project Structure

```
JiraLite/
├── Application/
│   ├── Dtos/              # Data Transfer Objects
│   │   ├── Comment/
│   │   ├── History/
│   │   ├── Issue/
│   │   ├── User/
│   │   └── Workflow/
│   ├── Interfaces/        # Service interfaces
│   └── Services/          # Business logic services
├── Controllers/           # MVC & API Controllers
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── IssuesController.cs
│   └── WorkflowController.cs
├── Domain/
│   └── Enums/            # Domain enumerations
├── Infrastructure/
│   ├── Data/             # Database context
│   └── Security/         # Authentication & authorization
├── Migrations/           # EF Core migrations
├── Models/              # Domain entities
│   ├── Base.cs
│   ├── Issue.cs
│   ├── IssueComment.cs
│   ├── IssueHistory.cs
│   ├── User.cs
│   └── WorkflowTransition.cs
├── Views/               # Razor views
│   ├── Account/
│   ├── Dashboard/
│   ├── Issues/
│   ├── Workflow/
│   └── Shared/
├── wwwroot/            # Static files
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
└── Program.cs          # Application entry point
```

## 📖 Usage

### 1. User Registration & Login

1. Navigate to https://localhost:7001
2. Click "Register" to create a new account
3. Fill in the registration form with:
   - Name
   - Email
   - Password
   - Role (User/Admin)
4. Login with your credentials

### 2. Creating Issues

**Via Web UI:**
1. Login and navigate to Dashboard
2. Click "Create Issue" button
3. Fill in issue details:
   - Title
   - Description
   - Type (Bug/Task)
   - Priority (High/Medium/Low)
4. Click Submit

**Via API:**
```bash
curl -X POST https://localhost:7001/api/issues \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Sample Issue",
    "description": "Issue description",
    "type": 1,
    "priority": 0
  }'
```

### 3. Bulk Upload from Excel

1. Navigate to "Issues" → "Bulk Upload"
2. Download the sample Excel template
3. Fill in your issues following the template format:
   - Title (required)
   - Description (required)
   - Type: Bug or Task
   - Priority: High, Medium, or Low
4. Upload the file
5. View success/failure results

### 4. Managing Workflows (Admin Only)

1. Login as Admin
2. Navigate to "Workflow" section
3. Create workflow transitions:
   - Select "From Status"
   - Select "To Status"
   - Add description (optional)
4. Save transition

### 5. Adding Comments

1. Open any issue detail page
2. Scroll to the comments section
3. Type your comment
4. Click "Add Comment"

### 6. Viewing History

1. Open any issue detail page
2. Click on the "History" tab
3. View all status transitions with timestamps

## 🔒 Security Features

- **Password Hashing:** BCrypt with salt
- **JWT Tokens:** Secure token-based authentication for APIs
- **Session Management:** Server-side session storage
- **Authorization Filters:** Role-based and session-based access control
- **CSRF Protection:** Anti-forgery tokens on forms
- **SQL Injection Prevention:** EF Core parameterized queries

## 🧪 Testing the API

### Using Swagger

1. Run the application
2. Navigate to https://localhost:7001/swagger
3. Click "Authorize" button
4. Enter: `Bearer YOUR_JWT_TOKEN`
5. Test endpoints interactively

### Getting a JWT Token

1. Register or login via:
   ```bash
   POST /api/auth/login
   {
     "email": "user@example.com",
     "password": "your_password"
   }
   ```

2. Copy the token from the response
3. Use it in the `Authorization` header:
   ```
   Authorization: Bearer YOUR_JWT_TOKEN
   ```

## 🎨 Customization

### Adding New Issue Types

1. Open `Domain/Enums/coreEnums.cs`
2. Add new type to `IssueType` enum:
```csharp
public enum IssueType
{
    Bug,
    Task,
    Feature,    // New type
    Story       // New type
}
```

3. Run migration:
```bash
dotnet ef migrations add AddNewIssueTypes
dotnet ef database update
```

### Adding New Status

1. Update `IssueStatus` enum in `Domain/Enums/coreEnums.cs`
2. Add new workflow transitions in the Workflow management page
3. Run migrations if needed

## 🐛 Troubleshooting

### Database Connection Issues

**Error:** `Unable to connect to MySQL server`

**Solution:**
1. Verify MySQL is running: `sudo systemctl status mysql`
2. Check connection string in `appsettings.json`
3. Verify user has proper permissions

### Migration Issues

**Error:** `Build failed`

**Solution:**
```bash
dotnet clean
dotnet build
dotnet ef database update
```

### JWT Token Issues

**Error:** `401 Unauthorized`

**Solution:**
1. Verify token is included in Authorization header
2. Check token hasn't expired (default: 24 hours)
3. Ensure JWT settings match in `appsettings.json`

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML comments for public methods
- Write unit tests for new features
- Update documentation

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Your Name** - *Initial work*

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Bootstrap team for the UI components
- EPPlus for Excel processing capabilities
- All contributors who help improve this project

## 📞 Support

For support, email support@jiralite.com or open an issue in the repository.

---

**Happy Issue Tracking! 🚀**
