using System;
using System.Linq;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Configuration;

namespace ProtoBuf.Grpc.Server
{
    /// <summary>
    /// Provides extension methods to the IServiceCollection API
    /// </summary>
    public static class ServicesExtensions
    {
        /// <summary>
        /// Registers a provider that can recognize and handle code-first services
        /// </summary>
        public static void AddCodeFirstGrpc(this IServiceCollection services) => AddCodeFirstGrpc(services, null);

        /// <summary>
        /// Registers a provider that can recognize and handle code-first services
        /// </summary>
        public static IGrpcServerBuilder AddCodeFirstGrpc(this IServiceCollection services, Action<GrpcServiceOptions>? configureOptions)
        {
            var builder = configureOptions == null ? services.AddGrpc() : services.AddGrpc(configureOptions);
            services.TryAddSingleton<IServiceImplementationMapper>(_ => new DependencyInjectionMapper(services));
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>), typeof(CodeFirstServiceMethodProvider<>)));
            return builder;
        }

        private sealed class DependencyInjectionMapper : IServiceImplementationMapper
        {
            private readonly IServiceCollection _serviceCollection;

            public DependencyInjectionMapper(IServiceCollection serviceCollection)
            {
                _serviceCollection = serviceCollection;
            }

            public Type Map(Type serviceType)
            {
                if (!serviceType.IsInterface)
                    return serviceType;

                var serviceImplementationType = _serviceCollection.SingleOrDefault(x => x.ServiceType == serviceType);
                return (serviceImplementationType is null) ? serviceType : serviceImplementationType.ImplementationType;
            }
        }

        private sealed class CodeFirstServiceMethodProvider<TService> : IServiceMethodProvider<TService> where TService : class
        {
            private readonly ILogger<CodeFirstServiceMethodProvider<TService>> _logger;
            private readonly BinderConfiguration? _binderConfiguration;
            private readonly IServiceImplementationMapper? _serviceImplementationMapper;
            public CodeFirstServiceMethodProvider(ILoggerFactory loggerFactory, IServiceImplementationMapper? serviceImplementationMapper = null, BinderConfiguration? binderConfiguration = null)
            {
                _binderConfiguration = binderConfiguration;
                _logger = loggerFactory.CreateLogger<CodeFirstServiceMethodProvider<TService>>();
                _serviceImplementationMapper = serviceImplementationMapper;
            }

            void IServiceMethodProvider<TService>.OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
            {
                int count = new Binder(_logger).Bind<TService>(context, _binderConfiguration, _serviceImplementationMapper);
                if (count != 0) _logger.Log(LogLevel.Information, "RPC services being provided by {0}: {1}", typeof(TService), count);
            }
        }
        private sealed class Binder : ServerBinder
        {
            private readonly ILogger _logger;
            internal Binder(ILogger logger)
                => _logger = logger;

            protected override void OnWarn(string message, object?[]? args)
                => _logger?.LogWarning(message, args ?? Array.Empty<object>());

            protected override void OnError(string message, object?[]? args = null)
                => _logger?.LogError(message, args ?? Array.Empty<object>());

            protected override bool TryBind<TService, TRequest, TResponse>(ServiceBindContext bindContext, Method<TRequest, TResponse> method, MethodStub<TService> stub)
                where TService : class
                where TRequest : class
                where TResponse : class
            {
                var metadata = bindContext.GetMetadata(stub.Method);

                var context = (ServiceMethodProviderContext<TService>)bindContext.State;
                switch (method.Type)
                {
                    case MethodType.Unary:
                        context.AddUnaryMethod(method, metadata, stub.CreateDelegate<UnaryServerMethod<TService, TRequest, TResponse>>());
                        break;
                    case MethodType.ClientStreaming:
                        context.AddClientStreamingMethod(method, metadata, stub.CreateDelegate<ClientStreamingServerMethod<TService, TRequest, TResponse>>());
                        break;
                    case MethodType.ServerStreaming:
                        context.AddServerStreamingMethod(method, metadata, stub.CreateDelegate<ServerStreamingServerMethod<TService, TRequest, TResponse>>());
                        break;
                    case MethodType.DuplexStreaming:
                        context.AddDuplexStreamingMethod(method, metadata, stub.CreateDelegate<DuplexStreamingServerMethod<TService, TRequest, TResponse>>());
                        break;
                    default:
                        return false;
                }
                return true;
            }
        }
    }
}