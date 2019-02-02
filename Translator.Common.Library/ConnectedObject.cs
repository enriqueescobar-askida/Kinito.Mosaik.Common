// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectedObject.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ConnectedObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Translator.Common.Library
{
    public abstract class ConnectedObject<T>
    {
        public T Connected
        {
            get;
            private set;
        }

        public void Connect(T connectedTo)
        {
            Connected = connectedTo;
        }
    }
}
