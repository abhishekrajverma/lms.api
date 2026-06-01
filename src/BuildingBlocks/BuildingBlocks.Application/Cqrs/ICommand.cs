using MediatR;
using SchoolErp.BuildingBlocks.Domain.Results;

namespace SchoolErp.BuildingBlocks.Application.Cqrs;

public interface ICommand<out TResponse> : IRequest<TResponse> where TResponse : Result;

public interface ICommand : IRequest<Result>;
