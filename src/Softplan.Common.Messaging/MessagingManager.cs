using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Softplan.Common.Messaging
{
    public class MessagingManager : IMessagingManager, IDisposable
    {
        private readonly ILogger logger;
        private readonly IList<IConsumer> consumers;
        private readonly ILoggerFactory loggerFactory;
        private bool active;

        public IList<IProcessor> EnabledProcessors { get; set; }
        public IBuilder Builder { get; }
        public IProcessorIgnorer ProcessorIgnorer { get; set; }
        public bool Active
        {
            get { return active; }
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        public MessagingManager(IBuilder builder, ILoggerFactory loggerFactory)
        {
            Builder = builder;
            this.loggerFactory = loggerFactory;

            logger = loggerFactory.CreateLogger<MessagingManager>();
            EnabledProcessors = new List<IProcessor>();
            consumers = new List<IConsumer>();
        }

        public MessagingManager()
        {
        }

        public void Start()
        {
            if (active)
            {
                logger.LogInformation("MQManager already started.");
                return;
            }

            try
            {
                logger.LogInformation("Starting MQManager.");
                active = true;
                StartConsumers();
                logger.LogInformation("MQManager started.");
            }
            catch (Exception err)
            {
                logger.LogError(err, "Error while starting MQManager");
                Stop();
            }
        }
        
        public void Stop()
        {
            if (!active)
            {
                return;
            }

            logger.LogInformation("Stopping MQManager");
            StopConsumers();
            active = false;
            logger.LogInformation("MQManager stopped");
        }

        public void RegisterProcessor(IProcessor processor)
        {
            EnabledProcessors.Add(processor);
            Builder.MessageQueueMap[processor.GetQueueName()] = processor.GetMessageType();
        }        

        private static IProcessor CreateProcessor(IServiceProvider serviceProvider, Type type)
        {
            return (from constructorInfo in type.GetConstructors() let args = GetConstructorDIArgs(constructorInfo, serviceProvider) where args.Count == constructorInfo.GetParameters().Count() select constructorInfo.Invoke(args.ToArray()) as IProcessor).FirstOrDefault();
        }                

        public void LoadProcessors(IServiceProvider serviceProvider)
        {
            var types = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.ListImplementationsOf<IProcessor>());

            foreach (var type in types)
            {
                if (ShouldIgnoreProcessor(type))
                {
                    continue;
                }

                var processor = CreateProcessor(serviceProvider, type);
                if (processor == null)
                {
                    logger.LogWarning($"Could not create a instance of {type} processor.");
                    continue;
                }

                RegisterProcessor(processor);
            }
        }

        private void StartConsumers()
        {
            foreach (var processor in EnabledProcessors)
            {
                processor.Logger = loggerFactory.CreateLogger(processor.GetType());
                var consumer = Builder.BuildConsumer();
                consumer.Start(processor, processor.GetQueueName());
                consumers.Add(consumer);
            }
        }

        private void StopConsumers()
        {
            foreach (var consumer in consumers)
            {
                consumer.Stop();
            }
            consumers.Clear();
        }
        
        private static IList<object> GetConstructorDIArgs(MethodBase ctor, IServiceProvider serviceProvider)
        {
            IList<object> args = new List<object>();

            foreach (var arg in ctor.GetParameters())
            {

                var svc = serviceProvider.GetService(arg.ParameterType);
                if (svc == null)
                {
                    if (!arg.HasDefaultValue)
                    {
                        return args;
                    }

                    svc = arg.DefaultValue;
                }

                args.Add(svc);
            }

            return args;
        }

        private bool ShouldIgnoreProcessor(Type type)
        {
            return ProcessorAlreadyRegistered(type) ||
                   ShouldIgnoreProcessorByType(type) ||
                   ProcessorExplicityIgnored(type);
        }

        private bool ProcessorAlreadyRegistered(Type type)
        {
            if (EnabledProcessors.FirstOrDefault(p => p.GetType() == type) == null) return false;
            logger.LogDebug($"Processor {type} is already registered.");
            return true;
        }

        private bool ShouldIgnoreProcessorByType(Type type)
        {
            if (!type.IsAbstract && type.IsClass && !type.IsNotPublic) return false;
            logger.LogDebug($"Processor {type} is not a valid, public processor type.");
            return true;
        }

        private bool ProcessorExplicityIgnored(Type type)
        {
            if (ProcessorIgnorer == null || !ProcessorIgnorer.ShouldIgnoreProcessorFrom(type)) return false;
            logger.LogDebug($"Processor of {type} was explicity ignored.");
            return true;
        }

        #region IDisposable Support
        private bool disposedValue; // Para detectar chamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;
            if (disposing)
            {
                StopConsumers();
            }

            disposedValue = true;
        }

        // Não altere este código. Coloque o código de limpeza em Dispose(bool disposing) acima.
        // Código adicionado para implementar corretamente o padrão descartável.
        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza em Dispose(bool disposing) acima.
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
