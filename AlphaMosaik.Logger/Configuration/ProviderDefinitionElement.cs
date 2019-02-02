using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Configuration;
using AlphaMosaik.Logger.Storage;
using AlphaMosaik.Logger.Core.Exceptions;

namespace AlphaMosaik.Logger.Configuration
{
    public class ProviderDefinitionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", Options = ConfigurationPropertyOptions.IsKey, DefaultValue = "")]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("displayName")]
        public string DisplayName
        {
            get
            {
                return (string)base["displayName"];
            }
            set
            {
                base["displayName"] = value;
            }
        }

        [ConfigurationProperty("description")]
        public string Description
        {
            get
            {
                return (string)base["description"];
            }
            set
            {
                base["description"] = value;
            }
        }

        [ConfigurationProperty("assembly", Options = ConfigurationPropertyOptions.IsRequired)]
        public string Assembly
        {
            get
            {
                return (string)base["assembly"];
            }
            set
            {
                base["assembly"] = value;
            }
        }

        [ConfigurationProperty("class", Options = ConfigurationPropertyOptions.IsRequired)]
        public string Class
        {
            get
            {
                return (string)base["class"];
            }
            set
            {
                base["class"] = value;
            }
        }

        public IStorageProvider CreateInstance()
        {
            try
            {
                ObjectHandle providerObjHandle = Activator.CreateInstance(Assembly, Class);

                if (providerObjHandle != null)
                {
                    IStorageProvider provider = (IStorageProvider)providerObjHandle.Unwrap();

                    if (provider != null)
                    {
                        return provider;
                    }
                }
                throw new ProviderDefinitionException("Impossible to create the instance. Check the provider definition.");

            }
            catch (Exception)
            {
                throw new ProviderDefinitionException("Impossible to create the instance. Check the provider definition.");
            }
        }
    }
}
