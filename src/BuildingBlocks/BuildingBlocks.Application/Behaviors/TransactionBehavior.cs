using MediatR;
using Microsoft.Extensions.Logging;

namespace SchoolErp.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Placeholder transaction behavior: repositories call SaveChangesAsync directly.
/// Does not use IUnitOfWork to avoid double-commit loops.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("TransactionBehavior pass-through for {Request}", typeof(TRequest).Name);
        return next();
    }
}
