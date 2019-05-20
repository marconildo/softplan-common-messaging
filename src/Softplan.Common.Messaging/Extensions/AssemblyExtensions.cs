using Softplan.Common.Messaging.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Softplan.Common.Messaging.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> ListImplementationsOf<T>(this Assembly assembly) where T : class
        {
            return assembly.GetLoadableTypes().Where(type => type.Implements<T>());
        }
        
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
