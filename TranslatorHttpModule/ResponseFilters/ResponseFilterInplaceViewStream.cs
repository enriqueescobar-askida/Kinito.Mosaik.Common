// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseFilterInplaceViewStream.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ResponseFilterInplaceViewStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Translator.Common.Library;
using TranslatorHttpHandler.TranslatorHelper;

namespace TranslatorHttpHandler.ResponseFilters
{
    public class ResponseFilterInplaceViewStream : Stream
    {
        private readonly Stream _responseStream;
        private readonly TranslatorHelper.TranslatorHelper _translatorHelper;
        private readonly string _languageCode;
        private readonly string _siteUrl;
        private readonly StringBuilder _html = new StringBuilder();
        private bool _disposed;

        public ResponseFilterInplaceViewStream(Stream responseStream, TranslatorHelper.TranslatorHelper translatorHelper, string languageCode, string siteUrl)
        {
            _responseStream = responseStream;
            _translatorHelper = translatorHelper;
            _languageCode = languageCode;
            _siteUrl = siteUrl;
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
                try
                {
                    _translatorHelper.TranslateInplaceView(_html, _languageCode, _responseStream, _siteUrl);
                }
                catch (Exception exc)
                {
                    string tempResponse = _html.ToString();
                    byte[] data = Encoding.UTF8.GetBytes(tempResponse);
                    _responseStream.Write(data, 0, data.Length);
                    Utilities.LogException("Write", exc, EventLogEntryType.Warning);
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
