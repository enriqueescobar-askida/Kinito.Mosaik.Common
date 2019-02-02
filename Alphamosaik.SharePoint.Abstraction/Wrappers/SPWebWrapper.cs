// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SPWebWrapper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SPWebWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.SharePoint.Abstraction.Wrappers
{
    using Microsoft.SharePoint;

    public class SPWebWrapper : SPWebBase
    {
        private readonly SPWeb _web;

        public SPWebWrapper(SPWeb web)
        {
            _web = web;
        }

        public SPWeb Web
        {
            get
            {
                return this._web;
            }
        }
    }
}