using FunctionExtensions.Result;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace FT.CQRS.Decorators
{
    public sealed class CommandLoggingDecorator<TCommand> : ICommandHandler<TCommand>
     where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _handler;
        private readonly ILogger<ICommandHandler<TCommand>> _logger;

        public CommandLoggingDecorator(ICommandHandler<TCommand> handler, ILogger<ICommandHandler<TCommand>> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public Result Handle(TCommand command)
        {
            string commandJson = JsonConvert.SerializeObject(command);

            // Use proper logging here
            _logger.LogDebug($"Type of command: {command.GetType().Name}: {commandJson}");

            var result = _handler.Handle(command);
            if (result.IsFailure)
            {
                _logger.LogWarning($"Result is failure: {result.Error}");
            }
            return result;
        }
    }
}