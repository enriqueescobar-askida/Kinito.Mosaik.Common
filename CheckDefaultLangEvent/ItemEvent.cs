// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemEvent.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ItemEvent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace CheckDefaultLangEvent
{
    public class ItemEvent : SPItemEventReceiver
    {
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            CheckDefaultLang(properties);
        }

        private static void CheckDefaultLang(SPItemEventProperties property)
        {
            try
            {
                SPListItem item = property.ListItem;
                                                
                SPList parentList = item.ParentList;
                
                SPListItemCollection listItemCollection = parentList.Items;
                
                if ((bool)item["DefaultLanguage"])
                {
                    foreach (SPListItem listItem in listItemCollection)
                    {
                        if ((bool)listItem["DefaultLanguage"] && (listItem.ID != item.ID))
                        {
                            listItem["DefaultLanguage"] = "false";                            
                            listItem.Update();                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in CheckDefaultLangEvent: " + ex.Message);
            }
        }
    }
}
