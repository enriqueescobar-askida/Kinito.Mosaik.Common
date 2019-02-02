// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlackList.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the BlackList type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;

namespace Alphamosaik.Oceanik.Caching
{
    public static class BlackList
    {
        private static readonly Hashtable List = new Hashtable
                                                     {
                                                         { "key1", "value1" } 
                                                     };

        public static bool IsBlackListed(string clientKey)
        {
            return List.ContainsKey(clientKey);
        }
    }
}