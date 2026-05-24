# ?? QUICK START GUIDE - EMS API

## ?? Prerequisites

```bash
? .NET 8 SDK installed
? SQL Server 2019+ installed
? Redis Server installed (or use Docker)
? Visual Studio 2022 or VS Code
? Git (optional)
```

---

## ?? **Installation Steps**

### **Step 1: Database Setup**

```bash
# Create database
sqlcmd -S your-server -U sa -P your-password

# Run SQL scripts in order
sqlcmd -S your-server -U sa -P your-password -i "001_CreateTables.sql"
sqlcmd -S your-server -U sa -P your-password -i "002_CreateIndexes.sql"
sqlcmd -S your-server -U sa -P your-password -i "003_sp_CalculateStudentGPA.sql"
sqlcmd -S your-server -U sa -P your-password -i "004_sp_GetStudentTranscript.sql"
sqlcmd -S your-server -U sa -P your-password -i "005_sp_EnrollStudent.sql"
sqlcmd -S your-server -U sa -P your-password -i "006_sp_GenerateGradeReport.sql"
sqlcmd -S your-server -U sa -P your-password -i "007_CreateRefreshTokensTable.sql"
sqlcmd -S your-server -U sa -P your-password -i "008_CreateTokenBlacklistTable.sql"
```

### **Step 2: Install NuGet Packages**

```bash
cd EMS.Api

# Install required packages
dotnet add package Dapper
dotnet add package StackExchange.Redis
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.MSSqlServer
dotnet add package FluentValidation
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package BCrypt.Net-Core
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.IdentityModel.Tokens
```

### **Step 3: Configure Connection Strings**

**File:** `EMS.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EMS_DB;Integrated Security=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

### **Step 4: Start Redis**

```bash
# Option 1: Docker
docker run -d -p 6379:6379 redis:latest

# Option 2: Windows Service
redis-server.exe

# Option 3: WSL
wsl redis-server
```

### **Step 5: Run Application**

```bash
cd EMS.Api
dotnet run
```

**API will be available at:** `http://localhost:5000`  
**Swagger UI:** `http://localhost:5000/swagger`

---

## ?? **First-Time Setup**

### **1. Register New User**

```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "ahmed",
    "email": "ahmed@example.com",
    "firstName": "Ahmed",
    "lastName": "Ali",
    "password": "SecurePassword123!",
    "role": "Student"
  }'
```

### **2. Login**

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "ahmed@example.com",
    "password": "SecurePassword123!"
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a7f3k9m2l8q0v1x6y4z...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": 1,
      "username": "ahmed",
      "email": "ahmed@example.com",
      "role": "Student"
    }
  }
}
```

### **3. Use Access Token**

```bash
curl -X GET http://localhost:5000/api/v1/auth/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

---

## ?? **Testing API Endpoints**

### **Using Postman**

1. **Create Environment**
   - Variable: `token` = (access token from login)
   - Variable: `baseUrl` = http://localhost:5000

2. **Use Token in Requests**
   - Header: `Authorization: Bearer {{token}}`

### **Using VS Code REST Client**

Create file: `.vscode/settings.json`

```json
{
  "rest-client.environmentVariables": {
    "$shared": {
      "baseUrl": "http://localhost:5000",
      "token": ""
    }
  }
}
```

---

## ?? **Common API Calls**

### **Get All Students** (Faculty/Admin only)
```bash
GET /api/v1/students?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

### **Create Student** (Faculty/Admin only)
```bash
POST /api/v1/students
Authorization: Bearer {token}
Content-Type: application/json

{
  "enrollmentNumber": "STU001",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "dateOfBirth": "2000-01-01T00:00:00Z",
  "departmentId": 1
}
```

### **Get Student by ID**
```bash
GET /api/v1/students/1
Authorization: Bearer {token}
```

### **Create Course** (Faculty/Admin only)
```bash
POST /api/v1/courses
Authorization: Bearer {token}
Content-Type: application/json

{
  "courseCode": "CS101",
  "name": "Introduction to Programming",
  "creditHours": 3,
  "departmentId": 1
}
```

### **Get All Courses**
```bash
GET /api/v1/courses?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

### **Enroll Student in Course**
```bash
POST /api/v1/courses/1/enroll/1
Authorization: Bearer {token}
```

### **Create Grade** (Faculty only)
```bash
POST /api/v1/grades
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentId": 1,
  "courseId": 1,
  "semester": "Fall 2024",
  "internalAssessmentScore": 85,
  "finalExamScore": 88,
  "finalPercentageScore": 86.5
}
```

### **Get Student Grades**
```bash
GET /api/v1/grades/student/1
Authorization: Bearer {token}
```

### **Get Student GPA**
```bash
GET /api/v1/grades/student/1/cumulative-gpa
Authorization: Bearer {token}
```

### **Mark Attendance** (Faculty only)
```bash
POST /api/v1/attendance
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentId": 1,
  "courseId": 1,
  "classDate": "2024-01-04T10:00:00Z",
  "status": "Present"
}
```

### **Get Attendance Summary**
```bash
GET /api/v1/attendance/summary/1/1
Authorization: Bearer {token}
```

### **Logout**
```bash
POST /api/v1/auth/logout
Authorization: Bearer {token}
```

---

## ?? **Debugging**

### **View Logs**

**File Location:**
```
EMS.Api/logs/ems-api-YYYY-MM-DD.txt
```

**Database Logs:**
```sql
SELECT TOP 100 * FROM [dbo].[Logs] ORDER BY [TimeStamp] DESC
```

### **Check Database Connection**

```csharp
// In Program.cs
var connection = new SqlConnection(connectionString);
connection.Open();
Console.WriteLine("Database connected!");
connection.Close();
```

### **Test Redis Connection**

```bash
# Verify Redis is running
redis-cli ping
# Should return: PONG
```

---

## ?? **Configuration**

### **Change JWT Secret**

**File:** `appsettings.json`

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-long-secret-key-at-least-32-characters",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### **Change Database**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=EMS_DB;User Id=sa;Password=YourPassword;"
  }
}
```

### **Change Redis Server**

```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-server:6379"
  }
}
```

---

## ?? **Troubleshooting**

### **Problem: Connection string not found**
```
? Solution: Check appsettings.json has correct format
```

### **Problem: JWT Secret too short**
```
? Solution: Make secret key at least 32 characters
```

### **Problem: Redis connection failed**
```
? Solution: Verify Redis is running and accessible
```

### **Problem: Database not found**
```
? Solution: Run SQL scripts to create tables
```

### **Problem: Port already in use**
```bash
# Change port in launchSettings.json
"applicationUrl": "https://localhost:5001;http://localhost:5001"
```

---

## ?? **Default Test Credentials**

**After setup, you can create users:**

```json
{
  "Admin": {
    "username": "admin",
    "email": "admin@example.com",
    "password": "AdminPass123!",
    "role": "Admin"
  },
  "Faculty": {
    "username": "faculty",
    "email": "faculty@example.com",
    "password": "FacultyPass123!",
    "role": "Faculty"
  },
  "Student": {
    "username": "student",
    "email": "student@example.com",
    "password": "StudentPass123!",
    "role": "Student"
  }
}
```

---

## ?? **Performance Tips**

1. **Enable query result caching**
   - Set Redis TTL appropriately
   - Cache frequently accessed data

2. **Use pagination**
   - Always use `?pageNumber=1&pageSize=10`
   - Never retrieve all records at once

3. **Monitor logs**
   - Check for slow queries (>1 second)
   - Monitor failed authentications

4. **Index optimization**
   - Database includes 17 key indexes
   - Monitor query execution plans

---

## ?? **Security Checklist**

- [ ] Change JWT secret key in production
- [ ] Use HTTPS in production
- [ ] Enable CORS only for trusted domains
- [ ] Monitor failed login attempts
- [ ] Regularly review audit logs
- [ ] Update dependencies regularly
- [ ] Use environment variables for secrets
- [ ] Implement rate limiting (next step)

---

## ?? **Support Resources**

- **Swagger API Docs:** `http://localhost:5000/swagger`
- **Health Check:** `http://localhost:5000/health`
- **Database Logs:** Check `[dbo].[Logs]` table
- **Application Logs:** Check `/logs/` folder

---

## ? **You're All Set!**

Your EMS API is ready to use. Start testing the endpoints and building your frontend!

**Happy coding! ??**
