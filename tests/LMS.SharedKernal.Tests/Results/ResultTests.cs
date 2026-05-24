using Xunit;
using LMS.SharedKernal.Results;
using FluentAssertions;

namespace LMS.SharedKernal.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var error = Error.Validation("name.empty", "Name cannot be empty.");
        var result = Result.Failure(error);
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessOfT_Value_ReturnsValue()
    {
        var result = Result.Success("hello");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void FailureOfT_AccessingValue_ThrowsInvalidOperationException()
    {
        var result = Result.Failure<string>(Error.NotFound("x", "Not found."));
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Value on a failed result*");
    }

    [Fact]
    public void Create_WithNonNullValue_ReturnsSuccess()
    {
        var result = Result.Create("value");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullValue_ReturnsFailureWithNullError()
    {
        var result = Result.Create<string>(null);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void Success_WithErrorOtherThanNone_ThrowsInvalidOperationException()
    {
        var act = () => new ResultTestHelper().CreateBadSuccess();
        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*successful result cannot have an error*");
    }

    [Fact]
    public void Failure_WithNoError_ThrowsInvalidOperationException()
    {
        var act = () => new ResultTestHelper().CreateBadFailure();
        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*failure result must have an error*");
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<int> result = 42;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}

public class ErrorTests
{
    [Fact]
    public void Validation_SetsCorrectType()
    {
        var error = Error.Validation("code", "desc");
        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("code");
        error.Description.Should().Be("desc");
    }

    [Fact]
    public void NotFound_SetsCorrectType()
    {
        var error = Error.NotFound("code", "desc");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Conflict_SetsCorrectType()
    {
        var error = Error.Conflict("code", "desc");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var error = Error.Validation("name.empty", "Name is empty.");
        error.ToString().Should().Be("[Validation] name.empty: Name is empty.");
    }

    [Fact]
    public void TwoErrorsWithSameValues_AreEqual()
    {
        var e1 = Error.Validation("code", "desc");
        var e2 = Error.Validation("code", "desc");
        e1.Should().Be(e2);
    }
}

internal class ResultTestHelper
{
    public Result CreateBadSuccess() =>
        (Result)Activator.CreateInstance(
            typeof(Result),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object[] { true, Error.Validation("x", "x") },
            null)!;

    public Result CreateBadFailure() =>
        (Result)Activator.CreateInstance(
            typeof(Result),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object[] { false, Error.None },
            null)!;
}