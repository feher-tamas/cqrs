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
            Result result = handler.Handle((dynamic)command);
            return result; 
        }

        public T Send<T>(IQuery<T> query)
        {
            Type type = typeof(IQueryHandler<,>);
            Type[] typeArgs = { query.GetType(), typeof(T) };
            Type handlerType = type.MakeGenericType(typeArgs);

            dynamic handler = _provider.GetService(handlerType);

            var methodInfo = handler.GetType().GetMethod("Handle");
            object[] parameters = new object[] { query };
            if (methodInfo != null)
            {
                return methodInfo.Invoke(handler, parameters);
            }
            throw new MethodAccessException();
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
                    var methodInfo = handler.GetType().GetMethod("Handle");
                    object[] parameters = new object[] {appevent};
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(handler, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}