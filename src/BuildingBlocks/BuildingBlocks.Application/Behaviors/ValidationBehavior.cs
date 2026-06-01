using FluentValidation;
using MediatR;
using SchoolErp.BuildingBlocks.Domain.Results;

namespace SchoolErp.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var message = string.Join("; ", failures.Select(f => f.ErrorMessage));
                return CreateFailure<TResponse>(Error.Validation(message));
            }
        }

        return await next();
    }

    private static TResponse CreateFailure<T>(Error error)
    {
        var resultType = typeof(T);
        if (resultType == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType.GetGenericArguments()[0]);
            return (TResponse)method.Invoke(null, [error])!;
        }

        throw new InvalidOperationException($"ValidationBehavior cannot create failure for {resultType.Name}.");
    }
}
