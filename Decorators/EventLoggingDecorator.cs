using FunctionExtensions.Result;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FT.CQRS.Decorators
{
    public sealed class EventLoggingDecorator<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
    {
        private readonly IEventHandler<TEvent> _handler;
        private readonly ILogger<IEventHandler<TEvent>> _logger;

        public EventLoggingDecorator(IEventHandler<TEvent> handler, ILogger<IEventHandler<TEvent>> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public Result Handle(TEvent @event)
        {
            string eventJson = JsonConvert.SerializeObject(@event);

            // Use proper logging here
            _logger.LogInformation($"Type of event:{@event.GetType().Name}: {eventJson}");

            Result result = _handler.Handle(@event);
            if (result.IsFailure)
            {
                _logger.LogWarning($"Result is failure: {result.Error}");
            }
            return result;
        }
    }
}