// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidConfigurationException.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Exception which occurs either when the config store is incorrectly configured or the requested config
//   item cannot be found.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Alphamosaik.Common.SharePoint.Library.ConfigStore
{
    /// <summary>
    /// Exception which occurs either when the config store is incorrectly configured or the requested config 
    /// item cannot be found.
    /// </summary>
    [Serializable]
    public class InvalidConfigurationException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        public InvalidConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public InvalidConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public InvalidConfigurationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
