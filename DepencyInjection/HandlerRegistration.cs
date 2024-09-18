using FT.CQRS;
using FT.CQRS.Decorators;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FT.CQRS.DependencyInjection
{
    public static class HandlerRegistration
    {
        public static void AddHandlers(this IServiceCollection services)
        {
            //List<Type> handlerTypes = typeof(ICommand).Assembly.GetTypes()
            //    .Where(x => x.GetInterfaces().Any(y => IsHandlerInterface(y)))
            //    .Where(x => x.Name.EndsWith("Handler"))
            //    .ToList();
            foreach (var assembyName in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                Assembly assembly = Assembly.Load(assembyName);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetInterfaces().Any(y => IsHandlerInterface(y)) && type.Name.EndsWith("Handler"))
                    {
                        AddHandler(services, type);
                    }
                }
            }
        }
        private static void AddHandler(IServiceCollection services, Type type)
        {
            object[] attributes = type.GetCustomAttributes(false);

            List<Type> pipeline = attributes
                .Select(x => ToDecorator(x))
                .Concat(new[] { type })
                .Reverse()
                .ToList();

            Type interfaceType = type.GetInterfaces().Single(y => IsHandlerInterface(y));
            Func<IServiceProvider, object> factory = BuildPipeline(pipeline, interfaceType);

            services.AddTransient(interfaceType, factory);
        }

        private static Func<IServiceProvider, object> BuildPipeline(List<Type> pipeline, Type interfaceType)
        {
            List<ConstructorInfo> ctors = pipeline
                .Select(x =>
                {
                    Type type = x.IsGenericType ? x.MakeGenericType(interfaceType.GenericTypeArguments) : x;
                    return type.GetConstructors().Single();
                })
                .ToList();

            Func<IServiceProvider, object> func = provider =>
            {
                object current = null;

                foreach (ConstructorInfo ctor in ctors)
                {
                    List<ParameterInfo> parameterInfos = ctor.GetParameters().ToList();

                    object[] parameters = GetParameters(parameterInfos, current, provider);

                    current = ctor.Invoke(parameters);
                }

#pragma warning disable CS8603 // Possible null reference return.
                return current;
#pragma warning restore CS8603 // Possible null reference return.
            };

            return func;
        }

        private static object[] GetParameters(List<ParameterInfo> parameterInfos, object current, IServiceProvider provider)
        {
            var result = new object[parameterInfos.Count];

            for (int i = 0; i < parameterInfos.Count; i++)
            {
                result[i] = GetParameter(parameterInfos[i], current, provider);
            }

            return result;
        }

        private static object GetParameter(ParameterInfo parameterInfo, object current, IServiceProvider provider)
        {
            Type parameterType = parameterInfo.ParameterType;

            if (IsHandlerInterface(parameterType))
                return current;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            object service = provider.GetService(parameterType);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (service != null)
                return service;

            throw new ArgumentException($"Type {parameterType} not found");
        }

        private static Type ToDecorator(object attribute)
        {
            Type type = attribute.GetType();

            //if (type == typeof(DatabaseRetryAttribute))
            //    return typeof(DatabaseRetryDecorator<>);

            if (type == typeof(CommandLogAttribute))
                return typeof(CommandLoggingDecorator<>);

            if (type == typeof(EventLogAttribute))
                return typeof(EventLoggingDecorator<>);
            if (type == typeof(QueryLogAttribute))
                return typeof(QueryLoggingDecorator<,>);
            /// other attributes go here

            if (attribute != null)
                throw new ArgumentException(attribute.ToString());
            else
                throw new ArgumentException("attribute can not be null");
        }

        private static bool IsHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefinition = type.GetGenericTypeDefinition();

            return typeDefinition == typeof(ICommandHandler<>) || typeDefinition == typeof(IQueryHandler<,>) || typeDefinition == typeof(IEventHandler<>);
        }
    }
}