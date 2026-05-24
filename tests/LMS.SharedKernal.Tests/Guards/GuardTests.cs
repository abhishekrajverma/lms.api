using Xunit;
using LMS.SharedKernal.Guards;
using FluentAssertions;

namespace LMS.SharedKernal.Tests.Guards;

public class GuardTests
{
    // ── AgainstNull ──────────────────────────────────────────────
    [Fact]
    public void AgainstNull_WithValue_ReturnsValue()
    {
        var result = Guard.AgainstNull("hello");
        result.Should().Be("hello");
    }

    [Fact]
    public void AgainstNull_WithNull_ThrowsArgumentNullException()
    {
        var act = () => Guard.AgainstNull<string>(null);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── AgainstNullOrEmpty ───────────────────────────────────────
    [Fact]
    public void AgainstNullOrEmpty_WithValue_ReturnsValue()
    {
        var result = Guard.AgainstNullOrEmpty("hello");
        result.Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrEmpty_WithBlankValue_ThrowsArgumentException(string? value)
    {
        var act = () => Guard.AgainstNullOrEmpty(value!);
        act.Should().Throw<ArgumentException>();
    }

    // ── AgainstEmptyGuid ─────────────────────────────────────────
    [Fact]
    public void AgainstEmptyGuid_WithValidGuid_ReturnsGuid()
    {
        var id = Guid.NewGuid();
        var result = Guard.AgainstEmptyGuid(id);
        result.Should().Be(id);
    }

    [Fact]
    public void AgainstEmptyGuid_WithEmptyGuid_ThrowsArgumentException()
    {
        var act = () => Guard.AgainstEmptyGuid(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be an empty Guid*");
    }

    // ── AgainstNegative (int) ────────────────────────────────────
    [Fact]
    public void AgainstNegative_Int_WithZero_Passes()
    {
        var result = Guard.AgainstNegative(0);
        result.Should().Be(0);
    }

    [Fact]
    public void AgainstNegative_Int_WithNegative_Throws()
    {
        var act = () => Guard.AgainstNegative(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── AgainstNegative (decimal) ────────────────────────────────
    [Fact]
    public void AgainstNegative_Decimal_WithPositive_Passes()
    {
        var result = Guard.AgainstNegative(9.99m);
        result.Should().Be(9.99m);
    }

    [Fact]
    public void AgainstNegative_Decimal_WithNegative_Throws()
    {
        var act = () => Guard.AgainstNegative(-0.01m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── AgainstZeroOrNegative ────────────────────────────────────
    [Fact]
    public void AgainstZeroOrNegative_WithPositive_Passes()
    {
        var result = Guard.AgainstZeroOrNegative(1);
        result.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AgainstZeroOrNegative_WithZeroOrNegative_Throws(int value)
    {
        var act = () => Guard.AgainstZeroOrNegative(value);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── AgainstExceedingMaxLength ─────────────────────────────────
    [Fact]
    public void AgainstExceedingMaxLength_WithinLimit_ReturnsValue()
    {
        var result = Guard.AgainstExceedingMaxLength("hello", 10);
        result.Should().Be("hello");
    }

    [Fact]
    public void AgainstExceedingMaxLength_ExceedsLimit_Throws()
    {
        var act = () => Guard.AgainstExceedingMaxLength("toolongstring", 5);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 5 characters*");
    }

    // ── AgainstInvalidEnum ────────────────────────────────────────
    private enum Status { Active = 1, Inactive = 2 }

    [Fact]
    public void AgainstInvalidEnum_WithValidValue_Passes()
    {
        var result = Guard.AgainstInvalidEnum(Status.Active);
        result.Should().Be(Status.Active);
    }

    [Fact]
    public void AgainstInvalidEnum_WithInvalidValue_Throws()
    {
        var act = () => Guard.AgainstInvalidEnum((Status)99);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*is not a valid value*");
    }
}
