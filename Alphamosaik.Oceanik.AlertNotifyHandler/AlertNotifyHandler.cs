using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Translator.Common.Library;

namespace Alphamosaik.Oceanik.AlertNotifyHandler
{
    class AlertNotifyHandler : IAlertNotifyHandler
    {
        #region IAlertNotifyHandler Members
  
        public bool OnNotification(SPAlertHandlerParams alertHandlerParams)
        {
            Debugger.Break();

            try
            {
                StringDictionary headers = alertHandlerParams.headers;
                string alertSubject = alertHandlerParams.headers["subject"].ToString();
                string alertBody = alertHandlerParams.body;
                
                SPSite site = new SPSite(alertHandlerParams.siteUrl+alertHandlerParams.webUrl);
                SPWeb web = site.OpenWeb();
                SPList list=web.Lists[alertHandlerParams.a.ListID];
                SPListItem item = list.GetItemById(alertHandlerParams.eventData[0].itemId) ;

                int alertLcid = (int)web.Language;
                string langIdStr = Languages.Instance.GetLanguageCode(alertLcid);

                string FullPath= HttpUtility.UrlPathEncode(alertHandlerParams.siteUrl+"/"+alertHandlerParams.webUrl+"/"+list.Title+"/"+item.Name);
                string ListPath = HttpUtility.UrlPathEncode(alertHandlerParams.siteUrl + "/" + alertHandlerParams.webUrl + "/" + list.Title);
                string webPath= HttpUtility.UrlPathEncode(alertHandlerParams.siteUrl+"/"+alertHandlerParams.webUrl);
 
                string build = "";
                
                string eventType = string.Empty;
                if (alertHandlerParams.eventData[0].eventType == 1)
                {
                    eventType = "Added";
                }
                else if (alertHandlerParams.eventData[0].eventType == 2)
                {
                    eventType = "Changed";
                }
                else if (alertHandlerParams.eventData[0].eventType == 3)
                {
                    eventType = "Deleted";
                }

                build = 
                    "<style type=\"text/css\">.style1 { font-size: small; border: 1px solid #000000;" +
                    "background-color: #DEE7FE;}.style2 { border: 1px solid #000000;}</style></head>" +                    
                    "<p><strong>" + item.Name.ToString() + "</strong> has been " + eventType +"</p>"+
                    "<table style=\"width: 100%\" class=\"style2\"><tr><td style=\"width: 25%\" class=\"style1\">" +
                    "<a href=" + webPath +"/_layouts/mysubs.aspx>Modify my Settings</a></td>" +
                    "<td style=\"width: 25%\" class=\"style1\"> <a href=" + FullPath + ">View " + item.Name + "</a></td>" +
                    "<td style=\"width: 25%\" class=\"style1\"><a href=" + ListPath + ">View " + list.Title + "</a></td>" +
                    "</tr></table>";
 
                string subject=list.Title.ToString() ;               

                // Hack temporaire
                alertHandlerParams.headers["to"] = "dominique.doucet@alphamosaik.com";
                
                SPUtility.SendEmail(web, true, false, alertHandlerParams.headers["to"].ToString(), subject,build);
                return false;
            }
            catch (System.Exception ex) 
            {
                Utilities.LogException("Error in Oceanik.AlertNotifyHandler: " + ex.Message, EventLogEntryType.Warning);
                return false;
            }
        }
        #endregion
   }
}