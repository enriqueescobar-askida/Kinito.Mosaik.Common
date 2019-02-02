// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseStaticOverride.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the BaseStaticOverride type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Alphamosaik.Common.SharePoint.Library
{
    public abstract class BaseStaticOverride<T> where T : BaseStaticOverride<T>
    {
        private static readonly object ObjectLock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (ObjectLock)
                    {
                        if (_instance == null)
                            _instance = (T)Activator.CreateInstance(typeof(T), true);
                    }
                }

                return _instance;
            }
        }
    }
}