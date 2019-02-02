using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace AlphaMosaik.Logger.Configuration
{
    public class LoggerSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled", DefaultValue = true)]
        [ConfigurationValidator(typeof(BooleanValidator))]
        public bool IsEnabled
        {
            get
            {
                return (bool)base["enabled"];
            }
            set
            {
                base["enabled"] = value;
            }
        }

        [ConfigurationProperty("logLevel", DefaultValue = "Information")]
        [ConfigurationValidator(typeof(EnumValidator))]
        public LogEntryLevel LogLevel
        {
            get
            {
                return (LogEntryLevel) base["logLevel"];
            }
            set
            {
                base["logLevel"] = value;
            }
        }

        [ConfigurationProperty("providerDefinitions", IsDefaultCollection = false, IsRequired = false)]
        public ProviderDefinitionCollection Definitions
        {
            get
            {
                return (ProviderDefinitionCollection)base["providerDefinitions"];
            }
        }

        [ConfigurationProperty("providerSettings", IsDefaultCollection = false, IsRequired = false)]
        public KeyValueConfigurationCollection ProviderSettings
        {
            get
            {
                return (KeyValueConfigurationCollection)base["providerSettings"];
            }
        }
    }

    public class BooleanValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            if(type == typeof(bool))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Validate(object value)
        {
            if(value is string)
            {
                bool result;
                if(!bool.TryParse((string) value, out result))
                {
                    throw new ArgumentException();
                }
            }
            else if(!(value is bool))
            {
                throw new ArgumentException();
            }
        }
    }
    public class EnumValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            if(type == typeof(LogEntryLevel))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Validate(object value)
        {
            if (value is string)
            {
                string valueAsString = (string) value;
                Enum.Parse(typeof (LogEntryLevel), valueAsString, true);
            }
            else if(!(value is LogEntryLevel))
            {
                throw new ArgumentException();
            }
        }
    }

}
