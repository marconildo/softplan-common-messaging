using System;

namespace Softplan.Common.Messaging.RabbitMq.Abstractions
{
    public interface IProcessorIgnorer
    {
        bool ShouldIgnoreProcessorFrom(Type type);
    }
}
