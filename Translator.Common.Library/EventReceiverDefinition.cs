// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventReceiverDefinition.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the EventReceiverDefinition type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.SharePoint;

namespace Translator.Common.Library
{
    public class EventReceiverDefinition
    {
        public SPEventReceiverType Type { get; set; }

        public string Assembly { get; set; }

        public string ClassName { get; set; }
    }
}