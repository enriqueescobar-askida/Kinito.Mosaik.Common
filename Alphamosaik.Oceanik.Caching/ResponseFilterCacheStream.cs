// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseFilterCacheStream.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ResponseFilterCacheStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Web;
using Alphamosaik.Common.Library;

namespace Alphamosaik.Oceanik.Caching
{
    public class ResponseFilterCacheStream : Stream
    {
        private readonly Stream _responseStream;
        private readonly StringBuilder _html = new StringBuilder();
        private bool _disposed;

        public ResponseFilterCacheStream(Stream responseStream)
        {
            _responseStream = responseStream;
        }

        public override bool CanRead
        {
            get { return _responseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _responseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _responseStream.CanWrite; }
        }

        public override long Length
        {
            get { return _responseStream.Length; }
        }

        public override long Position
        {
            get { return _responseStream.Position; }
            set { _responseStream.Position = value; }
        }

        public override void Flush()
        {
            if (_html.Length > 0)
            {
                // "<div class=\"OceanikPageCache"
                const string OceanikPageCache = "class=\"OceanikPageCache";

                int index = _html.IndexOf(OceanikPageCache, StringComparison.OrdinalIgnoreCase);

                if (index != -1)
                {
                    string noCache = _html.Substring(index, OceanikPageCache.Length + 2);

                    HttpContext.Current.Items["OceanikPageCache"] = (noCache.IndexOf("No") == -1);
                }
            }

            _responseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _responseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _responseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _responseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _html.Append(Encoding.UTF8.GetString(buffer, offset, count));
            _responseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _responseStream.Dispose();
                    _disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
