using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlphaMosaik.Logger
{
    public class LoggerConfigurationException : Exception
    {
        public LoggerConfigurationException() : base()
        {
        }

        public LoggerConfigurationException(string message) : base(message)
        {
        }

        public LoggerConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
