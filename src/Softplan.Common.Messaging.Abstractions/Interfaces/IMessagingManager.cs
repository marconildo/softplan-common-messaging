using System;
using System.Collections.Generic;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessagingManager
    {
        IList<IProcessor> EnabledProcessors { get; set; }
        IBuilder Builder { get; }
        bool Active { get; set; }
        void LoadProcessors(IServiceProvider serviceProvider);
        void RegisterProcessor(IProcessor processor);
        void Start();
        void Stop();
    }
}
