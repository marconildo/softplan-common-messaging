using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Softplan.Common.Messaging.Abstractions
{
    public class QueueInfo
    {
        private const string MaxPriorityHeader = "x-max-priority";
        
        public bool Durable { get; set; }
        [DataMember(Name = "auto_delete")]
        public bool AutoDelete { get; set; }
        public bool Exclusive { get; set; }
        [JsonIgnore]
        public int Priority { get; set; }
        public IDictionary<string, object> Arguments { get; }
        
        public QueueInfo()
        {
            Arguments = new Dictionary<string, object>();
        }        

        [OnSerializing()]
        private void OnSerializing(StreamingContext context)
        {
            Arguments[MaxPriorityHeader] = Priority;
        }

        [OnDeserialized()]
        private void OnDeSerialized(StreamingContext context)
        {
            Priority = Arguments.ContainsKey(MaxPriorityHeader) ? Convert.ToInt32(Arguments[MaxPriorityHeader]) : 0;
        }
    }
}
