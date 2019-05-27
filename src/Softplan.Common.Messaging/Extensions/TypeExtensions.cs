using System;
using System.Linq;
using System.Reflection;

namespace Softplan.Common.Messaging.Extensions
{
    public static class TypeExtensions
    {
        public static bool Implements<T>(this Type type)
        {
            return ((TypeInfo) type).ImplementedInterfaces.Contains(typeof(T));
        }
    }
}
