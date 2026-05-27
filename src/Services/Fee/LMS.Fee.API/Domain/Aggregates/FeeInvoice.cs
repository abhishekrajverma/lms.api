using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Fee.API.Domain.Aggregates;

public enum FeePaymentStatus
{
    Paid = 0,
    Pending = 1,
    Overdue = 2
}

public sealed class FeeInvoice : TenantAggregateRoot
{
    public Guid StudentId { get; private set; }
    public string StudentName { get; private set; } = string.Empty;
    public string ClassName { get; private set; } = string.Empty;
    public decimal TotalFee { get; private set; }
    public decimal PaidAmount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    private FeeInvoice(Guid id, Guid tenantId) : base(id, tenantId) { }
    private FeeInvoice() { }

    public decimal PendingAmount => Math.Max(0, TotalFee - PaidAmount);

    public FeePaymentStatus Status
    {
        get
        {
            if (IsPaid || PendingAmount <= 0)
                return FeePaymentStatus.Paid;
            if (DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
                return FeePaymentStatus.Overdue;
            return FeePaymentStatus.Pending;
        }
    }

    public static FeeInvoice Create(
        Guid tenantId,
        Guid studentId,
        string studentName,
        string className,
        decimal totalFee,
        DateOnly dueDate,
        decimal paidAmount = 0)
    {
        Guard.AgainstEmptyGuid(tenantId);
        Guard.AgainstEmptyGuid(studentId);
        Guard.AgainstNullOrEmpty(studentName);
        Guard.AgainstNegative(totalFee);
        if (paidAmount < 0 || paidAmount > totalFee)
            throw new ArgumentOutOfRangeException(nameof(paidAmount));

        var invoice = new FeeInvoice(Guid.NewGuid(), tenantId)
        {
            StudentId = studentId,
            StudentName = studentName.Trim(),
            ClassName = className.Trim(),
            TotalFee = totalFee,
            PaidAmount = paidAmount,
            DueDate = dueDate,
            IsPaid = paidAmount >= totalFee,
            CreatedAtUtc = DateTime.UtcNow,
            PaidAtUtc = paidAmount > 0 ? DateTime.UtcNow : null
        };
        return invoice;
    }

    public void MarkPaid()
    {
        PaidAmount = TotalFee;
        IsPaid = true;
        PaidAtUtc = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount)
    {
        Guard.AgainstNegative(amount);
        PaidAmount = Math.Min(TotalFee, PaidAmount + amount);
        if (PaidAmount >= TotalFee)
            MarkPaid();
    }
}
