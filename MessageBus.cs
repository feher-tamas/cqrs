using FunctionExtensions.Result;
using Microsoft.Extensions.DependencyInjection;

namespace FT.CQRS
{
    public class MessageBus : IBus
    {
        private readonly IServiceProvider _provider;

        public MessageBus(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Result Send(ICommand command)
        {
            Type type = typeof(ICommandHandler<>);
            Type[] typeArgs = { command.GetType() };
            Type handlerType = type.MakeGenericType(typeArgs);
            dynamic handler = _provider.GetService(handlerType);
            Result result = handler?.Handle((dynamic)command);
            return result; 
        }

        public T Send<T>(IQuery<T> query)
        {
            Type type = typeof(IQueryHandler<,>);
            Type[] typeArgs = { query.GetType(), typeof(T) };
            Type handlerType = type.MakeGenericType(typeArgs);

            dynamic handler = _provider.GetService(handlerType);
            T result = handler?.Handle((dynamic)query);
            return result;
        }

        public void Send(IEnumerable<IEvent> events)
        {
            foreach (IEvent ev in events)
            {
                Send(ev);
            }
        }

        public void Send(IEvent appevent)
        {
            try
            {
                Type type = typeof(IEventHandler<>);
                Type[] typeArgs = { appevent.GetType() };
                Type handlerType = type.MakeGenericType(typeArgs);
                dynamic handlers = _provider.GetServices(handlerType);
                foreach (dynamic handler in handlers)
                {
                    handler.Handle((dynamic)appevent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}