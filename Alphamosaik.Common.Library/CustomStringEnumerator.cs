// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomStringEnumerator.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the CustomStringEnumerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;

namespace Alphamosaik.Common.Library
{
    public class CustomStringEnumerator
    {
        // Fields
        private readonly IEnumerator _baseEnumerator;
        private readonly IEnumerable _temp;

        // Methods
        internal CustomStringEnumerator(IEnumerable mappings)
        {
            _temp = mappings;
            _baseEnumerator = _temp.GetEnumerator();
        }

        // Properties
        public string Current
        {
            get
            {
                return (string)_baseEnumerator.Current;
            }
        }

        public bool MoveNext()
        {
            return _baseEnumerator.MoveNext();
        }

        public void Reset()
        {
            _baseEnumerator.Reset();
        }
    }
}