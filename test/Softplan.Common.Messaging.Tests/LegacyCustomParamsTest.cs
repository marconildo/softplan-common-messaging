using System.Collections.Generic;
using Xunit;
using Softplan.Common.Messaging;

namespace Softplan.Common.Messaging.UnitTest
{
    public class LegacyCustomParamsTest
    {

        public static IEnumerable<object[]> ToDictData()
        {
            return new List<object[]> {
                new object[] { new List<string> {"a=a", "b=b"}, new Dictionary<string, string> { {"a","a"}, {"b", "b"} }},
                new object[] { new List<string> {"a=", "b"}, new Dictionary<string, string> { {"a",""}, {"b", ""} }},
                new object[] { new List<string> { "a=SuperMegaTeste=2", "b={\"teste\": \"teste\"}"},
                    new Dictionary<string, string> { {"a","SuperMegaTeste=2"}, {"b", "{\"teste\": \"teste\"}"}} },
            };
        }

        public static IEnumerable<object[]> FromDictData()
        {
            return new List<object[]> {
                new object[] { new Dictionary<string, string> { {"a","a"}, {"b", "b"} }, new List<string> {"a=a", "b=b"}},
                new object[] { new Dictionary<string, string> { {"a",""}, {"b", ""} }, new List<string> {"a=", "b="}},
                new object[] { new Dictionary<string, string> { {"a","SuperMegaTeste=2"}, {"b", "{\"teste\": \"teste\"}"}} ,
                    new List<string> { "a=SuperMegaTeste=2", "b={\"teste\": \"teste\"}"} },
            };
        }

        [Theory]
        [MemberData(nameof(ToDictData))]
        public void ConvertToDictWithDataTest(IList<string> data, Dictionary<string, string> target)
        {
            var par = new LegacyCustomParams();
            foreach (var s in data) { par.Items.Add(s); };
            var dict = new Dictionary<string, string>();
            par.ToDictionary(dict);
            Assert.Equal(target, dict);
        }

        [Theory]
        [MemberData(nameof(FromDictData))]
        public void ConvertFromDictWithDataTest(Dictionary<string, string> data, IList<string> target)
        {
            var par = new LegacyCustomParams();
            par.FromDictionary(data);
            Assert.Equal(target, par.Items);
        }
    }
}
