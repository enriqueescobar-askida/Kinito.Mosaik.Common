// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SPContextStatic.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SPContextStatic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.SharePoint.Abstraction
{
    using System;
    using System.Web;

    public class SPContextStatic
    {
        // Methods
        public static SPContextBase GetContext<T>(SPWebBase web)
        {
            return ((T)typeof(T)).GetContext(web);
        }

        public static SPContextBase GetContext<T>(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext<T>(HttpContext context, SPItemBase item, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext<T>(HttpContext context, int itemId, Type itemType)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext<T>(HttpContext context, Guid viewId, Guid listId, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext<T>(HttpContext context, int itemId, Guid listId, SPWebBase web)
        {
            throw new NotImplementedException();
        }

        public static SPContextBase GetContext<T>(HttpContext context, string itemId, Guid listId, SPWebBase web)
        {
            throw new NotImplementedException();
        }
    }
}