using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace AlphaMosaik.Logger.Configuration
{
    public class ProviderDefinitionCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("defaultProvider", IsRequired = false)]
        public string DefaultProvider
        {
            get
            {
                return (string) base["defaultProvider"];
            }
            set
            {
                base["defaultProvider"] = value;
            }
        }

        public ProviderDefinitionElement this[int idx]
        {
            get
            {
                return (ProviderDefinitionElement) BaseGet(idx);
            }
        }

        public ProviderDefinitionElement this[string name]
        {
            get
            {
                return (ProviderDefinitionElement) BaseGet(name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderDefinitionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderDefinitionElement) element).Name; // On indique que l'attribut "Name" est la clé des éléments
        }
    }
}
