namespace LMS.Fee.API.Application.DTOs;

public sealed record FeeInvoiceDto(
    Guid Id,
    Guid StudentId,
    string Student,
    string Class,
    decimal TotalFee,
    decimal Paid,
    decimal Pending,
    string DueDate,
    string Status);

public sealed record FeeSummaryDto(
    decimal TotalCollected,
    decimal TotalPending,
    decimal OverdueAmount);

public sealed record MonthlyFeeCollectionDto(string Month, decimal Collected, decimal Pending);

public sealed record RecentFeePaymentDto(
    Guid Id,
    string Student,
    string Class,
    decimal Amount,
    string Date,
    string Status);

public sealed record AdmissionTrendDto(string Month, int Admissions);
