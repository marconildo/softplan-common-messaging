using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Softplan.Common.Messaging.RabbitMq.Abstractions
{
    public class LegacyCustomParams
    {
        public LegacyCustomParams()
        {
            Items = new List<string>();
        }

        [JsonProperty("items")]
        public IList<string> Items { get; set; }

        public void ToDictionary(IDictionary<string, string> dict)
        {
            dict.Clear();
            foreach (var value in Items)
            {
                var index = value.IndexOf("=", StringComparison.Ordinal);
                if (index >= 0)
                {
                    dict[value.Substring(0, index)] = value.Substring(index + 1);
                }
                else
                {
                    dict[value] = "";
                }
            }
        }

        public void FromDictionary(IDictionary<string, string> dict)
        {
            Items.Clear();
            foreach (var kv in dict)
            {
                Items.Add($"{kv.Key}={kv.Value}");
            }
        }
    }
}
