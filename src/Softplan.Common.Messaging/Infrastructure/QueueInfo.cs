using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Softplan.Common.Messaging.Infrastructure
{
    public class QueueInfo
    {
        const string maxPriorityHeader = "x-max-priority";
        public QueueInfo()
        {
            Arguments = new Dictionary<string, object>();
        }

        public bool Durable { get; set; }

        [DataMember(Name = "auto_delete")]
        public bool AutoDelete { get; set; }

        public bool Exclusive { get; set; }
        [JsonIgnore]
        public Int32 Priority { get; set; }

        public IDictionary<string, object> Arguments { get; private set; }

        [OnSerializing()]
        private void OnSerializing(StreamingContext context)
        {
            Arguments[maxPriorityHeader] = Priority;
        }

        [OnDeserialized()]
        private void OnDeSerialized(StreamingContext context)
        {
            Priority = Arguments.ContainsKey(maxPriorityHeader) ? Convert.ToInt32(Arguments[maxPriorityHeader]) : 0;
        }
    }
}
