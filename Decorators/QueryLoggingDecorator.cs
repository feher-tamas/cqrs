using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FT.CQRS.Decorators
{
    public sealed class QueryLoggingDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
     where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> _handler;
        private readonly ILogger<IQueryHandler<TQuery, TResult>> _logger;

        public QueryLoggingDecorator(IQueryHandler<TQuery, TResult> handler, ILogger<IQueryHandler<TQuery, TResult>> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public TResult Handle(TQuery query)
        {
            string queryJson = JsonConvert.SerializeObject(query);

            _logger.LogInformation($"Type of query: {query.GetType().Name}: {queryJson}");
            var result = _handler.Handle(query);
            string resultJson = JsonConvert.SerializeObject(result);
            _logger.LogInformation($"Result is : {resultJson}");
            return result;
        }
    }
}