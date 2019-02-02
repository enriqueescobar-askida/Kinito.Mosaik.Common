using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using AlphaMosaik.Logger.Storage;
using System.Reflection;

namespace AlphaMosaik.Logger.Configuration
{
    partial class StorageProviderDefinitions
    {
        public StorageProviderDefinitions()
        {
            this.providersField = new ProviderDefinition[0];
            this.defaultProviderField = string.Empty;
        }
    }

    partial class ProviderDefinition
    {
        public IStorageProvider CreateInstance()
        {
            Assembly providerAssembly = System.Reflection.Assembly.Load(this.Assembly);
            ObjectHandle providerObjHandle = Activator.CreateInstance(this.Assembly, this.Class);

            if (providerObjHandle != null)
            {
                IStorageProvider provider = (IStorageProvider)providerObjHandle.Unwrap();

                if (provider != null)
                {
                    return provider;
                }
            }
            throw new ArgumentException("Impossible to create the instance. Check the provider definition.");
        }
    }
}
