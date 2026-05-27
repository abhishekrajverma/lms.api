using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Domain.Aggregates;

namespace LMS.Fee.API.Application;

internal static class FeeMappings
{
    public static FeeInvoiceDto ToDto(FeeInvoice invoice) =>
        new(
            invoice.Id,
            invoice.StudentId,
            invoice.StudentName,
            invoice.ClassName,
            invoice.TotalFee,
            invoice.PaidAmount,
            invoice.PendingAmount,
            invoice.DueDate.ToString("yyyy-MM-dd"),
            invoice.Status.ToString().ToLowerInvariant());

    public static RecentFeePaymentDto ToRecentPayment(FeeInvoice invoice) =>
        new(
            invoice.Id,
            invoice.StudentName,
            invoice.ClassName,
            invoice.PaidAmount > 0 ? invoice.PaidAmount : invoice.TotalFee,
            (invoice.PaidAtUtc ?? invoice.CreatedAtUtc).ToString("yyyy-MM-dd"),
            invoice.IsPaid ? "completed" : "pending");
}
