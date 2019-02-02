// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexedList.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IndexedList type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace Translator.Common.Library
{
    public class IndexedList<T> : IEnumerable<KeyValuePair<int, List<T>>>
    {
        // represent Dictionary<Hashcode, List<ID>>
        internal readonly Dictionary<int, List<T>> Index;

        public IndexedList(int initCount)
        {
            Index = new Dictionary<int, List<T>>(initCount);
        }

        public virtual void IndexItem(T id, string item)
        {
            int hashCode = item.GetHashCode();

            List<T> list;

            if (Index.TryGetValue(hashCode, out list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<T> { id };
                Index[hashCode] = list;
            }
        }

        public bool TryGetValue(int hashCode, out List<T> list)
        {
            return Index.TryGetValue(hashCode, out list);
        }

        public void Clear()
        {
            Index.Clear();
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<int, List<T>>> GetEnumerator()
        {
            return Index.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
