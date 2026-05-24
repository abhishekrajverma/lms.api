using Xunit;
using LMS.SharedKernal.Primitives;
using FluentAssertions;

namespace LMS.SharedKernal.Tests.Primitives;

public class ValueObjectTests
{
    private class Email : ValueObject
    {
        public string Value { get; }
        public Email(string value) => Value = value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLower();
        }
    }

    private class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }
        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency.ToUpper();
        }
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var e1 = new Email("test@test.com");
        var e2 = new Email("TEST@TEST.COM");
        e1.Should().Be(e2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var e1 = new Email("a@test.com");
        var e2 = new Email("b@test.com");
        e1.Should().NotBe(e2);
    }

    [Fact]
    public void Equals_DifferentTypes_ReturnsFalse()
    {
        var email = new Email("test@test.com");
        var money = new Money(10, "USD");
        email.Should().NotBe(money);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var e1 = new Email("test@test.com");
        var e2 = new Email("test@test.com");
        e1.GetHashCode().Should().Be(e2.GetHashCode());
    }

    [Fact]
    public void Money_SameAmountAndCurrency_AreEqual()
    {
        var m1 = new Money(99.99m, "usd");
        var m2 = new Money(99.99m, "USD");
        m1.Should().Be(m2);
    }

    [Fact]
    public void Money_DifferentAmount_AreNotEqual()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(20m, "USD");
        m1.Should().NotBe(m2);
    }

    [Fact]
    public void OperatorEquality_SameValues_ReturnsTrue()
    {
        var e1 = new Email("x@y.com");
        var e2 = new Email("x@y.com");
        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void OperatorInequality_DifferentValues_ReturnsTrue()
    {
        var e1 = new Email("a@y.com");
        var e2 = new Email("b@y.com");
        (e1 != e2).Should().BeTrue();
    }
}
