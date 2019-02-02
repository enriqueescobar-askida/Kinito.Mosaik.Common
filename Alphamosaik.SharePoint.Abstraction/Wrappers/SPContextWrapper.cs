// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SPContextWrapper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SPContextBaseWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.SharePoint.Abstraction.Wrappers
{
    using System;
    using System.Web;

    using Microsoft.SharePoint;

    public class SPContextWrapper : SPContextBase
    {
        // Fields
        private readonly SPContext _context;

        public SPContextWrapper(SPContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            _context = httpContext;
        }

        // Properties
        public override bool ContentTypeSetInQueryString
        {
            get { return _context.ContentTypeSetInQueryString; }

            set { _context.ContentTypeSetInQueryString = value; }
        }

        public override SPContextPageInfoBase ContextPageInfo
        {
            get
            {
                throw new NotImplementedException();
            }

            internal set
            {
                throw new NotImplementedException();
            }
        }

        public override SPContextBase Current
        {
            get
            {
                return new SPContextWrapper(_context);
            }
        }

        public override GetCachedFieldBase FieldControlCacheGetCallback
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override SetCachedFieldBase FieldControlCacheSetCallback
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override SPFieldCollectionBase Fields
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SPFileBase File
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SPFileLevelBase FileLevel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SPFormContextBase FormContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool HasDesignTimeContentType
        {
            get { return _context.HasDesignTimeContentType; }
        }

        public override bool IsDesignTime
        {
            get { return _context.IsDesignTime; }
        }

        public override bool IsPopUI
        {
            get { return _context.IsPopUI; }
        }

        public override bool IsRemoteAuthoringTime
        {
            get { return _context.IsRemoteAuthoringTime; }
        }

        public override SPItemBase Item
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int ItemId
        {
            get { return _context.ItemId; }

            set { _context.ItemId = value; }
        }

        public override string ItemIdAsString
        {
            get { return _context.ItemIdAsString; }

            set { _context.ItemIdAsString = value; }
        }

        public override SPListBase List
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Guid ListId
        {
            get { return _context.ListId; }
        }

        public override SPListItemBase ListItem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string ListItemDisplayName
        {
            get { return _context.ListItemDisplayName; }

            set { _context.ListItemDisplayName = value; }
        }

        public override string ListItemServerRelativeUrl
        {
            get { return _context.ListItemServerRelativeUrl; }

            set { _context.ListItemServerRelativeUrl = value; }
        }

        public override SPListItemVersionBase ListItemVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool LoadContentTypes
        {
            get { return _context.LoadContentTypes; }

            set { _context.LoadContentTypes = value; }
        }

        public override SPMobileContextBase MobileContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string RecurrenceID
        {
            get { return _context.RecurrenceID; }
        }

        public override SPRegionalSettingsBase RegionalSettings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string RootFolderUrl
        {
            get { return _context.RootFolderUrl; }
        }

        public override SPSiteBase Site
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SPFeatureCollectionBase SiteFeatures
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SPSiteSubscriptionBase SiteSubscription
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool UseDefaultCachePolicy
        {
            get { return _context.UseDefaultCachePolicy; }

            set { _context.UseDefaultCachePolicy = value; }
        }

        public override SPViewContextBase ViewContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SPWebBase Web
        {
            get
            {
                return new SPWebWrapper(_context.Web);
            }
        }

        public override SPFeatureCollectionBase WebFeatures
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // Methods
        public static SPContextBase GetContext(SPWebBase web)
        {
            return new SPContextWrapper(SPContext.GetContext(((SPWebWrapper)web).Web));
        }

        public static SPContextBase GetContext(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext(HttpContext context, SPItemBase item, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext(HttpContext context, int itemId, Type itemType)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext(HttpContext context, Guid viewId, Guid listId, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext(HttpContext context, int itemId, Guid listId, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext(HttpContext context, string itemId, Guid listId, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public override bool GetValueFromPageData(string strKey, out object objValue)
        {
            return _context.GetValueFromPageData(strKey, out objValue);
        }

        public override void ResetItem()
        {
            _context.ResetItem();
        }
    }
}