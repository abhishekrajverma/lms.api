using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Academic.API.Domain.Aggregates;

public enum TransportStatus
{
    Active = 0,
    Maintenance = 1,
    Delayed = 2,
    Inactive = 3
}

public sealed class TransportRoute : TenantAggregateRoot
{
    public string RouteName { get; private set; } = string.Empty;
    public string BusNumber { get; private set; } = string.Empty;
    public string DriverName { get; private set; } = string.Empty;
    public string DriverPhone { get; private set; } = string.Empty;
    public int StudentCount { get; private set; }
    public int Capacity { get; private set; }
    public TransportStatus Status { get; private set; } = TransportStatus.Active;
    public string Location { get; private set; } = string.Empty;
    public string Eta { get; private set; } = string.Empty;
    public string LastUpdate { get; private set; } = string.Empty;

    private TransportRoute(Guid id, Guid tenantId) : base(id, tenantId) { }
    private TransportRoute() { }

    public static TransportRoute Create(
        Guid tenantId,
        string routeName,
        string busNumber,
        string driverName,
        string driverPhone,
        int studentCount,
        int capacity,
        TransportStatus status = TransportStatus.Active,
        string? location = null,
        string? eta = null)
    {
        Guard.AgainstEmptyGuid(tenantId);
        return new TransportRoute(Guid.NewGuid(), tenantId)
        {
            RouteName = routeName.Trim(),
            BusNumber = busNumber.Trim(),
            DriverName = driverName.Trim(),
            DriverPhone = driverPhone.Trim(),
            StudentCount = studentCount,
            Capacity = capacity,
            Status = status,
            Location = location ?? string.Empty,
            Eta = eta ?? string.Empty,
            LastUpdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
        };
    }
}
