// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseBufferElement.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ResponseBufferElement type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace Alphamosaik.Oceanik.Caching
{
    [Serializable]
    internal class ResponseBufferElement : IResponseElement
    {
        private readonly byte[] _data;
        private readonly int _size;
        private int _free;

        internal ResponseBufferElement(byte[] data, int size)
        {
            _data = data;
            _size = size;
            _free = 0;
        }

        public byte[] GetBytes()
        {
            return _data;
        }

        public long GetSize()
        {
            return _size - _free;
        }

        internal ResponseBufferElement Clone()
        {
            int count = _size - _free;
            var dst = new byte[count];
            Buffer.BlockCopy(_data, 0, dst, 0, count);
            return new ResponseBufferElement(dst, count);
        }

        internal int Append(byte[] data, int offset, int size)
        {
            if ((_free == 0) || (size == 0))
            {
                return 0;
            }

            int count = (_free >= size) ? size : _free;
            Buffer.BlockCopy(data, offset, _data, _size - _free, count);
            _free -= count;
            return count;
        }

        internal void AppendEncodedChars(char[] data, int offset, int size, Encoder encoder, bool flushEncoder)
        {
            int num = encoder.GetBytes(data, offset, size, _data, _size - _free, flushEncoder);
            _free -= num;
        }
    }
}
