# LMS API — School ERP Backend

Microservices backend for the **school-erp-dashboard** Next.js frontend. All tenant-scoped APIs require:

```http
X-Tenant-Id: {tenant-guid}
```

## Quick start

```powershell
docker compose up -d
# Gateway: http://localhost:5000
# Direct ports (override): Tenant 5011, Student 5012, Academic 5013, Fee 5014, Notification 5015
```

1. Register a school (no tenant header):

```http
POST http://localhost:5011/api/tenants
Content-Type: application/json

{ "name": "Sunrise International School", "subdomain": "sis" }
```

2. Use returned `id` as `X-Tenant-Id` for all other calls.

## Gateway routes

| Frontend need | Gateway URL | Downstream |
|---------------|-------------|------------|
| Schools list | `GET /tenants/schools` | `GET /api/schools` |
| Dashboard stats | `GET /api/dashboard/stats` | BFF (aggregates services) |
| Students | `GET /students/students?search=&class=&status=` | Student API |
| Teachers | `GET /academic/teachers` | Academic API |
| Fees | `GET /fees/fees/records` | Fee API |
| Attendance | `GET /students/attendance/summary` | Student API |
| Transport | `GET /academic/transport/routes` | Academic API |
| Payroll | `GET /academic/payroll/records` | Academic API |
| Reports | `GET /academic/reports/revenue` | Academic API |
| Notices | `GET /notifications/notices` | Notification API |

## API reference (direct service paths)

### Tenant (`/api`)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/tenants` | Register school |
| GET | `/tenants` | List active tenants |
| GET | `/tenants/{id}` | Get tenant |
| GET | `/schools` | List schools (dashboard shape) |

### Student (`/api`, requires tenant)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/students` | List students (search, class, status, page) |
| GET | `/students/summary` | Totals for dashboard |
| GET | `/students/{id}` | Student detail |
| POST | `/students` | Create student |
| GET | `/attendance/summary` | Attendance overview |
| GET | `/attendance/students` | Per-student attendance rows |
| GET | `/attendance/heatmap` | Daily heatmap data |
| GET | `/attendance/weekly` | Mon–Fri chart |
| GET | `/admissions/recent` | Recent admissions widget |
| GET | `/dashboard/admissions` | Admission trend chart |

### Fee (`/api`, requires tenant)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/fees/records` | Fee records list |
| GET | `/fees/summary` | Collected / pending / overdue |
| GET | `/fees/collection/monthly` | Monthly collection chart |
| GET | `/fees/payments/recent` | Recent payments widget |
| POST | `/fees` | Create invoice |
| POST | `/fees/{id}/pay` | Mark paid |

### Academic (`/api`, requires tenant)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/classes` | List classes |
| POST | `/classes` | Create class |
| GET | `/teachers` | List teachers |
| GET | `/teachers/summary` | Teacher stats |
| POST | `/teachers` | Hire teacher |
| GET | `/transport/routes` | Transport routes |
| GET | `/transport/summary` | Transport stats |
| GET | `/payroll/records` | Payroll list |
| GET | `/payroll/summary` | Payroll totals |
| GET | `/payroll/approvals/pending` | Pending approvals widget |
| GET | `/exams/upcoming` | Upcoming exams |
| GET | `/timetable/today` | Today's classes |
| GET | `/attendance/teachers/today` | Teacher attendance widget |
| GET | `/reports/revenue` | Revenue report |
| GET | `/reports/expenses` | Expense categories |
| GET | `/reports/academics/classes` | Class performance |
| GET | `/reports/academics/subjects` | Subject performance |
| GET | `/reports/attendance/trend` | Attendance trend |
| GET | `/reports/revenue-growth` | Revenue vs expenses |
| GET | `/reports/salary-distribution` | Salary by department |

### Notification (`/api`, requires tenant)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/notifications` | Header notification list |
| GET | `/notices` | School notices board |

### Gateway BFF

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/dashboard/stats` | Aggregated dashboard KPIs |

## Frontend integration

In `school-erp-dashboard`, add:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

Example fetch:

```ts
const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/students/students`, {
  headers: { 'X-Tenant-Id': tenantId },
})
const { items, total } = await res.json()
```

Response shapes match `lib/data.ts` field names (camelCase JSON).

## Sample create payloads

**Student**

```json
{
  "firstName": "Arjun",
  "lastName": "Sharma",
  "admissionNumber": "1001",
  "className": "10-A",
  "email": "arjun.s@school.edu",
  "phone": "+91 98765 43210",
  "status": "active",
  "feeStatus": "paid",
  "attendancePercentage": 96
}
```

**Fee invoice**

```json
{
  "studentId": "{guid}",
  "studentName": "Arjun Sharma",
  "className": "10-A",
  "totalFee": 120000,
  "dueDate": "2024-06-30",
  "paidAmount": 120000
}
```

**Teacher**

```json
{
  "firstName": "Anita",
  "lastName": "Singh",
  "department": "Mathematics",
  "subject": "Mathematics",
  "email": "anita.s@school.edu",
  "phone": "+91 98765 12341",
  "salary": 72000,
  "status": "active"
}
```
