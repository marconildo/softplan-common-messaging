using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Softplan.Common.Messaging.Properties;

namespace Softplan.Common.Messaging
{
    public class MessagingManager : IMessagingManager, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IList<IConsumer> _consumers;
        private readonly ILoggerFactory _loggerFactory;
        private bool _active;

        public IList<IProcessor> EnabledProcessors { get; set; }
        public IBuilder Builder { get; }
        public IProcessorIgnorer ProcessorIgnorer { get; set; }
        public bool Active
        {
            get => _active;
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
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MessagingManager>();
            EnabledProcessors = new List<IProcessor>();
            _consumers = new List<IConsumer>();
        }

        public void Start()
        {
            if (IsActive()) return;

            try
            {
                _logger.LogInformation(Resources.MQManagerStarting);
                _active = true;
                StartConsumers();
                _logger.LogInformation(Resources.MQManagerStarted);
            }
            catch (Exception err)
            {
                _logger.LogError(err, Resources.MQManagerErrorWhileStarting);
                Stop();
            }
        }        

        public void Stop()
        {
            if (!_active)
            {
                return;
            }

            _logger.LogInformation(Resources.MQManagerStopping);
            StopConsumers();
            _active = false;
            _logger.LogInformation(Resources.MQManagerStopped);
        }

        public void RegisterProcessor(IProcessor processor)
        {
            EnabledProcessors.Add(processor);
            Builder.MessageQueueMap[processor.GetQueueName()] = processor.GetMessageType();
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
                    _logger.LogWarning(string.Format(Resources.ErrorToCreateProcessorInstance, type));
                    continue;
                }

                RegisterProcessor(processor);
            }
        }
        
        private bool IsActive()
        {
            if (!_active) return false;
            _logger.LogInformation(Resources.MQManagerAlreadyStarted);
            return true;
        }

        private void StartConsumers()
        {
            foreach (var processor in EnabledProcessors)
            {
                processor.Logger = _loggerFactory.CreateLogger(processor.GetType());
                var consumer = Builder.BuildConsumer();
                consumer.Start(processor, processor.GetQueueName());
                _consumers.Add(consumer);
            }
        }

        private void StopConsumers()
        {
            foreach (var consumer in _consumers)
            {
                consumer.Stop();
            }
            _consumers.Clear();
        }                

        private bool ShouldIgnoreProcessor(Type type)
        {
            return ProcessorAlreadyRegistered(type) ||
                   ShouldIgnoreProcessorByType(type) ||
                   ProcessorExplicitlyIgnored(type);
        }

        private bool ProcessorAlreadyRegistered(Type type)
        {
            if (EnabledProcessors.FirstOrDefault(p => p.GetType() == type) == null) return false;
            _logger.LogDebug(string.Format(Resources.ProcessorAlreadyResgistered, type));
            return true;
        }

        private bool ShouldIgnoreProcessorByType(Type type)
        {
            if (!type.IsAbstract && type.IsClass && !type.IsNotPublic) return false;
            _logger.LogDebug(string.Format(Resources.ProcessorIsInvalid, type));
            return true;
        }

        private bool ProcessorExplicitlyIgnored(Type type)
        {
            if (ProcessorIgnorer == null || !ProcessorIgnorer.ShouldIgnoreProcessorFrom(type)) return false;
            _logger.LogDebug(string.Format(Resources.ProcessorWasIgnored, type));
            return true;
        }
        
        private static IProcessor CreateProcessor(IServiceProvider serviceProvider, Type type)
        {
            return (from constructorInfo in type.GetConstructors()
                let args = GetConstructorDiArgs(constructorInfo, serviceProvider)
                where args.Count == constructorInfo.GetParameters().Count()
                select constructorInfo.Invoke(args.ToArray()) as IProcessor).FirstOrDefault();
        } 
        
        private static IList<object> GetConstructorDiArgs(MethodBase methodBase, IServiceProvider serviceProvider)
        {
            IList<object> args = new List<object>();
            foreach (var arg in methodBase.GetParameters())
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

        #region IDisposable Support
        private bool _disposedValue; // Para detectar chamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                StopConsumers();
            }

            _disposedValue = true;
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
