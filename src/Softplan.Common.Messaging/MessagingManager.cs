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
        private bool active = false;

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

        private IList<object> GetConstructorDIArgs(ConstructorInfo ctor, IServiceProvider serviceProvider)
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

        private IProcessor CreateProcessor(IServiceProvider serviceProvider, Type type)
        {

            foreach (var ctor in type.GetConstructors())
            {
                var args = GetConstructorDIArgs(ctor, serviceProvider);

                if (args.Count != ctor.GetParameters().Count())
                {
                    continue;
                }

                return ctor.Invoke(args.ToArray()) as IProcessor;
            }
            return null;
        }

        private bool ShouldIgnoreProcessor(Type type)
        {
            if (EnabledProcessors.FirstOrDefault(p => p.GetType() == type) != null)
            {
                logger.LogDebug($"Processor {type} is already registered.");
                return true;
            }

            if (type.IsAbstract || !type.IsClass || type.IsNotPublic)
            {
                logger.LogDebug($"Processor {type} is not a valid, public processor type.");
                return true;
            }

            if (ProcessorIgnorer != null && ProcessorIgnorer.ShouldIgnoreProcessorFrom(type))
            {
                logger.LogDebug($"Processor of {type} was explicity ignored.");
                return true;
            }

            return false;
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

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar chamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopConsumers();
                }

                disposedValue = true;
            }
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
