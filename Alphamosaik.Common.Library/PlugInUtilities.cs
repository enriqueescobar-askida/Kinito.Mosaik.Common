using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Alphamosaik.Common.Library
{
    public class PlugInUtilities
    {
        static public T GetPlugin<T>(string fullAssemblyName)
        {
            T t = default(T);

            Debug.Assert(typeof(T).IsInterface);

            try
            {
                Type type = Type.GetType(fullAssemblyName);

                if (!type.IsClass || type.IsNotPublic)
                    return t;

                Type[] interfaces = type.GetInterfaces();

                if (((IList)interfaces).Contains(typeof(T)))
                {
                    object obj = Activator.CreateInstance(type);
                    t = (T)obj;
                    return t;
                }

                /*var assemblyName = new AssemblyName(fullAssemblyName);
                Assembly assembly = Assembly.Load(fullAssemblyName);

                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsNotPublic)
                        continue;

                    Type[] interfaces = type.GetInterfaces();

                    if (((IList)interfaces).Contains(typeof(T)))
                    {
                        object obj = Activator.CreateInstance(type);
                        t = (T)obj;
                        return t;
                    }
                }*/
            }
            catch (Exception ex)
            {
                // Hide error
                Trace.WriteLine(ex.Message);
            }

            return t;
        }
    }
}
