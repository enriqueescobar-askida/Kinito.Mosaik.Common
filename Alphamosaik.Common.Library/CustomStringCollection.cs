// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomStringCollection.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StringEnumeratorAlpha type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;

namespace Alphamosaik.Common.Library
{
    [Serializable]
    public sealed class CustomStringCollection : IList
    {
        // Fields
        private readonly ArrayList _data = new ArrayList();

        public CustomStringCollection()
        {
            _data = new ArrayList();
        }

        public CustomStringCollection(int capacity)
        {
            _data = new ArrayList(capacity);
        }

        // Properties
        public int Capacity
        {
            get { return _data.Capacity; }

            set { _data.Capacity = value; }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return _data.SyncRoot; }
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        public string this[int index]
        {
            get { return (string)_data[index]; }

            set { _data[index] = value; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }

            set { this[index] = (string)value; }
        }

        // Methods
        public int Add(string value)
        {
            return _data.Add(value);
        }

        public void AddRange(string[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            _data.AddRange(value);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(string value)
        {
            return _data.Contains(value);
        }

        public void CopyTo(string[] array, int index)
        {
            _data.CopyTo(array, index);
        }

        public void CopyTo(CustomStringCollection dest)
        {
            foreach (string s in _data)
            {
                dest.Add(s);
            }
        }

        public CustomStringEnumerator GetEnumerator()
        {
            return new CustomStringEnumerator(this);
        }

        public int IndexOf(string value)
        {
            return _data.IndexOf(value);
        }

        public void Insert(int index, string value)
        {
            _data.Insert(index, value);
        }

        public void Remove(string value)
        {
            _data.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _data.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return Add((string)value);
        }

        bool IList.Contains(object value)
        {
            return Contains((string)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((string)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (string)value);
        }

        void IList.Remove(object value)
        {
            Remove((string)value);
        }
    }
}
