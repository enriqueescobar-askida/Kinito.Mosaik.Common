using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AlphaMosaik.Logger.Core.Exceptions
{
    public class ProviderDefinitionException : Exception
    {
        public ProviderDefinitionException() : base()
        {}

        public ProviderDefinitionException(string message): base(message)
        {}

        public ProviderDefinitionException(string message, Exception innerException) : base(message, innerException)
        {}

        public ProviderDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}
