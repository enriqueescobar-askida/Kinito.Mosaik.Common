// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateItemAutoTranslationEvent.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the UpdateItemAutoTranslationEvent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Web;
using Alphamosaik.Oceanik.AutomaticTranslation;
using Alphamosaik.Oceanik.Sdk;
using Microsoft.SharePoint;
using Translator.Common.Library;
using System.Diagnostics;

namespace UpdateItemAutoTranslation
{
    public class UpdateItemAutoTranslationEvent : SPItemEventReceiver
    {
        private const string OceanikAutomaticTranslation = "Oceanik.AutomaticTranslation";

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            TranslateFields(properties);
            EventFiringEnabled = true;
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            EventFiringEnabled = false;
            TranslateFields(properties);
            EventFiringEnabled = true;
        }

        private static void TranslateFields(SPItemEventProperties property)
        {   
            using (SPWeb currentWeb = property.OpenWeb())
            {
                currentWeb.AllowUnsafeUpdates = true;
                string listId = property.ListId.ToString();
                string itemId = property.ListItem.ID.ToString();
                const string Lang = "ALL";
                string url = currentWeb.Url;
                
                try
                {
                    SPList currentList = property.ListItem.ParentList;
                    SPListItem currentItem = property.ListItem;
                    bool discussionBoardEdition = ((property.EventType == SPEventReceiverType.ItemUpdated) && (currentList.BaseTemplate == SPListTemplateType.DiscussionBoard));

                    if (currentList.Fields.ContainsField("ItemsAutoCreation"))
                    {
                        if (currentItem["ItemsAutoCreation"] != null)
                        {
                            if ((currentItem["ItemsAutoCreation"].ToString() == "Create items for all languages") || 
                                (currentItem["ItemsAutoCreation"].ToString() == "Overwrite/Create items for all languages"))
                            {
                                TranslatorAutoTranslation.CreateClonedMultilingualItem(
                                    HttpRuntime.Cache[OceanikAutomaticTranslation] as IAutomaticTranslation, currentWeb,
                                    listId, url, itemId, Lang, false, discussionBoardEdition);

                                if (currentList.BaseTemplate != SPListTemplateType.DiscussionBoard)
                                {
                                    currentWeb.AllowUnsafeUpdates = true;
                                    currentItem["ItemsAutoCreation"] = "None";
                                    if (currentItem["AutoTranslation"] != null)
                                    {
                                        currentItem["AutoTranslation"] = "No";
                                    }

                                    currentItem.SystemUpdate(false);
                                }                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogException("Error in UpdateItemAutoTranslation: " + ex.Message, ex, EventLogEntryType.Warning);
                }

                currentWeb.AllowUnsafeUpdates = false;
            }
        }
    }
}
