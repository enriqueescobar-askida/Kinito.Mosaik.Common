// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpApplicationEvents.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IHttpApplicationEvents type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace TranslatorHttpHandler.ApplicationEvents
{
    public interface IHttpApplicationEvents
    {
        void ContextBeginRequest(object sender, EventArgs e);

        void ContextEndRequest(object sender, EventArgs e);

        void ContextReleaseRequestState(object sender, EventArgs e);

        void OnPreProcessRequest(object sender, EventArgs eventArgs);
    }
}
