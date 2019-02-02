// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenerateId.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the GenerateId type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace CreateGUIDEvent
{
    public class GenerateId : SPItemEventReceiver
    {
        public override void ItemAdded(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            CreateGuid(properties);
            EventFiringEnabled = true;
        }

        private static void CreateGuid(SPItemEventProperties property)
        {
            try
            {
                SPListItem item = property.ListItem;
                string newGuid = Guid.NewGuid().ToString();
                item["Title"] = newGuid;

                item.Update();
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in creating the guid in the event handler: " + ex.Message);
            }
        }
    }
}
