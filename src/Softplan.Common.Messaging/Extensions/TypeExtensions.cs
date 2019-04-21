using System;
using System.Linq;

namespace Softplan.Common.Messaging.Extensions
{
    public static class TypeExtensions
    {
        public static bool Implements<T>(this Type type)
        {
            return (type as System.Reflection.TypeInfo).ImplementedInterfaces.Contains(typeof(T));
        }
    }
}
