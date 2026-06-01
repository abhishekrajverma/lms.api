using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.SharedKernel;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invalid phone number.", nameof(value));
        return new PhoneNumber(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents() => [Value];

    public override string ToString() => Value;
}
