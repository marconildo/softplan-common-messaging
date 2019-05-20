using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Softplan.Common.Messaging.Extensions
{
    public static class AssemblyExtensions
    {
        private const string ParamName = "assembly";
        
        public static IEnumerable<Type> ListImplementationsOf<T>(this Assembly assembly) where T : class
        {
            return assembly.GetLoadableTypes().Where(type => type.Implements<T>());
        }

        private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {                
                throw new ArgumentNullException(ParamName);
            }

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
