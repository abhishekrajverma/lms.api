namespace SchoolErp.BuildingBlocks.Domain;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(IBusinessRule rule) : base(rule.Message) { }
}
