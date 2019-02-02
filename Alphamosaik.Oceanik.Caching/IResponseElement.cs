// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IResponseElement.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IResponseElement type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.Caching
{
    internal interface IResponseElement
    {
        // Methods
        byte[] GetBytes();

        long GetSize();
    }
}
