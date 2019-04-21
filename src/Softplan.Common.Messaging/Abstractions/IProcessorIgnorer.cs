using System;

namespace Softplan.Common.Messaging.Abstractions
{
    public interface IProcessorIgnorer
    {
        bool ShouldIgnoreProcessorFrom(Type type);
    }
}
