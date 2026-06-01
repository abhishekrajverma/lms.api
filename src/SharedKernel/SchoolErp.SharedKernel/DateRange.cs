using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.SharedKernel;

public sealed class DateRange : ValueObject
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ArgumentException("End must be on or after start.");
        Start = start;
        End = end;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
