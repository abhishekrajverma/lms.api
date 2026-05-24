using System.Runtime.CompilerServices;

namespace LMS.SharedKernal.Guards;

public static class Guard
{
    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string paramName = "") where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"'{paramName}' cannot be null.");

        return value;
    }

    public static string AgainstNullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' cannot be null or empty.", paramName);

        return value;
    }

    public static Guid AgainstEmptyGuid(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"'{paramName}' cannot be an empty Guid.", paramName);

        return value;
    }

    public static int AgainstNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' cannot be negative.");

        return value;
    }

    public static decimal AgainstNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' cannot be negative.");

        return value;
    }

    public static int AgainstZeroOrNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' must be greater than zero.");

        return value;
    }

    public static string AgainstExceedingMaxLength(
        string value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        AgainstNullOrEmpty(value, paramName);

        if (value.Length > maxLength)
            throw new ArgumentException(
                $"'{paramName}' cannot exceed {maxLength} characters. Current length: {value.Length}.",
                paramName);

        return value;
    }

    public static T AgainstInvalidEnum<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string paramName = "") where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException($"'{value}' is not a valid value for enum '{typeof(T).Name}'.", paramName);

        return value;
    }
}
