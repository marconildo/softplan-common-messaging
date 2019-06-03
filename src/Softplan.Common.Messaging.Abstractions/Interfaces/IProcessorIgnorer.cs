using System;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IProcessorIgnorer
    {
        bool ShouldIgnoreProcessorFrom(Type type);
    }
}
