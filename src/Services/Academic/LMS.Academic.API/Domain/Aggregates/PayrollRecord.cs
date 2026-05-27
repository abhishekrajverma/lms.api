using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public enum PayrollStatus
{
    Pending = 0,
    Approved = 1,
    Paid = 2
}

public sealed class PayrollRecord : TenantAggregateRoot
{
    public string EmployeeName { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public decimal BasicSalary { get; private set; }
    public decimal Allowances { get; private set; }
    public decimal Deductions { get; private set; }
    public PayrollStatus Status { get; private set; } = PayrollStatus.Pending;
    public string PayMonth { get; private set; } = string.Empty;

    private PayrollRecord(Guid id, Guid tenantId) : base(id, tenantId) { }
    private PayrollRecord() { }

    public decimal NetSalary => BasicSalary + Allowances - Deductions;

    public static PayrollRecord Create(
        Guid tenantId,
        string employeeName,
        string department,
        decimal basicSalary,
        decimal allowances,
        decimal deductions,
        string payMonth,
        PayrollStatus status = PayrollStatus.Pending)
    {
        Guard.AgainstEmptyGuid(tenantId);
        return new PayrollRecord(Guid.NewGuid(), tenantId)
        {
            EmployeeName = employeeName.Trim(),
            Department = department.Trim(),
            BasicSalary = basicSalary,
            Allowances = allowances,
            Deductions = deductions,
            PayMonth = payMonth.Trim(),
            Status = status
        };
    }

    public void Approve() => Status = PayrollStatus.Approved;
    public void MarkPaid() => Status = PayrollStatus.Paid;
}
