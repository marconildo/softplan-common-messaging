using System;

namespace Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces
{
    public interface IProcessorIgnorer
    {
        bool ShouldIgnoreProcessorFrom(Type type);
    }
}
