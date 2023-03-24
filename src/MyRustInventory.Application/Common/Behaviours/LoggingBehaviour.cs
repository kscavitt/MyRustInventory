using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MyRustInventory.Application.Common.Behaviours
{
    //public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    //{
    //    private readonly ILogger _logger;
    //    private readonly ICurrentUserService _currentUserService;
    //    private readonly IIdentityService _identityService;

    //    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService, IIdentityService identityService)
    //    {
    //        _logger = logger;
    //        _currentUserService = currentUserService;
    //        _identityService = identityService;
    //    }

    //    public async Task Process(TRequest request, CancellationToken cancellationToken)
    //    {
    //        var requestName = typeof(TRequest).Name;
    //        var userId = _currentUserService.UserId ?? string.Empty;
    //        string? userName = string.Empty;

    //        if (!string.IsNullOrEmpty(userId))
    //        {
    //            userName = await _identityService.GetUserNameAsync(userId);
    //        }

    //        _logger.LogInformation("MyRustInventory Request: {Name} {@UserId} {@UserName} {@Request}",
    //            requestName, userId, userName, request);
    //    }
    //}

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
            => _logger = logger;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"MyRustInventory Request: {typeof(TRequest).Name}");
            var response = await next();

            _logger.LogInformation($"MyRustInventory Request: {typeof(TResponse).Name}");

            return response;
        }
    }

}

