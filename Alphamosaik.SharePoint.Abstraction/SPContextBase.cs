// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SPContextBase.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SPContextBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Alphamosaik.SharePoint.Abstraction
{
    public abstract class SPContextBase
    {
        public Func<SPContextBase, SPWebBase> GetContext;

        // Properties
        public virtual bool ContentTypeSetInQueryString
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

        public virtual SPContextPageInfoBase ContextPageInfo
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
        
        public virtual SPContextBase Current
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual GetCachedFieldBase FieldControlCacheGetCallback
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

        public virtual SetCachedFieldBase FieldControlCacheSetCallback
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

        public virtual SPFieldCollectionBase Fields
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual SPFileBase File
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual SPFileLevelBase FileLevel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual SPFormContextBase FormContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool HasDesignTimeContentType
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual bool IsDesignTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual bool IsPopUI
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual bool IsRemoteAuthoringTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual SPItemBase Item
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual int ItemId
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

        public virtual string ItemIdAsString
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
        
        public virtual SPListBase List
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual Guid ListId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual SPListItemBase ListItem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual string ListItemDisplayName
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

        public virtual string ListItemServerRelativeUrl
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

        public virtual SPListItemVersionBase ListItemVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual bool LoadContentTypes
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

        public virtual SPMobileContextBase MobileContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual string RecurrenceID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual SPRegionalSettingsBase RegionalSettings
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual string RootFolderUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual SPSiteBase Site
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual SPFeatureCollectionBase SiteFeatures
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual SPSiteSubscriptionBase SiteSubscription
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool UseDefaultCachePolicy
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

        public virtual SPViewContextBase ViewContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public virtual SPWebBase Web
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual SPFeatureCollectionBase WebFeatures
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // Methods
        public virtual bool GetValueFromPageData(string strKey, out object objValue)
        {
            throw new NotImplementedException();
        }

        public virtual void ResetItem()
        {
            throw new NotImplementedException();
        }
    }
}
