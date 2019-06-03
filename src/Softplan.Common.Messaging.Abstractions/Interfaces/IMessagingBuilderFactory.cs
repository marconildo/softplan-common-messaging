using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessagingBuilderFactory
    {
        bool HasConfiguration(IConfiguration config);
        IBuilder GetBuilder(IConfiguration config, ILoggerFactory loggerFactory);
    }
}