using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace LMS.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public DashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var tenantId = Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { code = "Tenant.Required", message = "Provide X-Tenant-Id header." });

        var client = _httpClientFactory.CreateClient("lms-services");
        client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

        var studentSummary = await GetJsonAsync<StudentsSummaryResponse>(
            client, "Student", "api/students/summary", cancellationToken);
        var teacherSummary = await GetJsonAsync<TeachersSummaryResponse>(
            client, "Academic", "api/teachers/summary", cancellationToken);
        var feeSummary = await GetJsonAsync<FeeSummaryResponse>(
            client, "Fee", "api/fees/summary", cancellationToken);
        var transportSummary = await GetJsonAsync<TransportSummaryResponse>(
            client, "Academic", "api/transport/summary", cancellationToken);
        var payrollSummary = await GetJsonAsync<PayrollSummaryResponse>(
            client, "Academic", "api/payroll/summary", cancellationToken);

        var stats = new
        {
            totalStudents = studentSummary?.Total ?? 0,
            totalTeachers = teacherSummary?.Total ?? 0,
            pendingFees = feeSummary?.TotalPending ?? 0,
            monthlyRevenue = feeSummary?.TotalCollected ?? 0,
            attendancePercentage = studentSummary?.AverageAttendance ?? 0,
            salaryPaid = payrollSummary?.Paid ?? 0,
            transportRoutes = transportSummary?.ActiveRoutes ?? 0,
            newAdmissions = studentSummary?.NewThisMonth ?? 0
        };

        return Ok(stats);
    }

    private async Task<T?> GetJsonAsync<T>(HttpClient client, string service, string path, CancellationToken ct)
    {
        var baseUrl = _configuration[$"Services:{service}"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return default;

        try
        {
            using var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}", ct);
            if (!response.IsSuccessStatusCode)
                return default;

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: ct);
        }
        catch
        {
            return default;
        }
    }

    private sealed record StudentsSummaryResponse(int Total, int Active, int NewThisMonth, decimal AverageAttendance);
    private sealed record TeachersSummaryResponse(int Total, int Active, int OnLeave, int Departments);
    private sealed record FeeSummaryResponse(decimal TotalCollected, decimal TotalPending, decimal OverdueAmount);
    private sealed record TransportSummaryResponse(int ActiveRoutes, int TotalStudents, int UnderMaintenance);
    private sealed record PayrollSummaryResponse(decimal Total, decimal Pending, decimal Paid);
}
