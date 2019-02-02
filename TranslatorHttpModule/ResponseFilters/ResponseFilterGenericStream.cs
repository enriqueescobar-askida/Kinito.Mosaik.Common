// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseFilterGenericStream.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ResponseFilterGeneric type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Alphamosaik.Common.Library.Licensing;
using Translator.Common.Library;
using TranslatorHttpHandler.TranslatorHelper;

namespace TranslatorHttpHandler.ResponseFilters
{
    public class ResponseFilterGenericStream : Stream
    {
        private readonly bool _pageToTranslate;
        private readonly string _languageCode = "EN";
        private readonly bool _viewAllItemsInEveryLanguages;
        private readonly bool _completingDictionaryMode;
        private readonly int _extractorStatus;
        private readonly string _url;
        private readonly Stream _responseStream;
        private readonly TranslatorHelper.TranslatorHelper _translatorHelper;
        private readonly StringBuilder _html = new StringBuilder();
        private readonly int _autoCompletionStatus;
        private readonly bool _mobilePage;
        private readonly License.LicenseType _licenseType;
        private bool _disposed;

        public ResponseFilterGenericStream(Stream inputStream, TranslatorHelper.TranslatorHelper translatorHelper, string languageCode, bool pageToTranslate, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, int extractorStatus, string url, int autoCompletionStatus, bool mobilePage, License.LicenseType licenseType)
        {
            _responseStream = inputStream;
            _translatorHelper = translatorHelper;
            _languageCode = languageCode;
            _pageToTranslate = pageToTranslate;
            _viewAllItemsInEveryLanguages = viewAllItemsInEveryLanguages;
            _completingDictionaryMode = completingDictionaryMode;
            _extractorStatus = extractorStatus;
            _url = url;
            _autoCompletionStatus = autoCompletionStatus;
            _mobilePage = mobilePage;
            _licenseType = licenseType;
        }

        public override long Position { get; set; }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override void Close()
        {
            _responseStream.Close();
            base.Close();
        }

        public override void Flush()
        {
            if (_html.Length > 0)
            {
                if (_pageToTranslate)
                {
                    try
                    {
                        _translatorHelper.Write(_html, _languageCode, _responseStream, _viewAllItemsInEveryLanguages, _completingDictionaryMode, _extractorStatus, _url, _autoCompletionStatus, _mobilePage, _licenseType);
                    }
                    catch (Exception exc)
                    {
                        string tempResponse = _html.ToString();
                        byte[] data = Encoding.UTF8.GetBytes(tempResponse);
                        _responseStream.Write(data, 0, data.Length);
                        Utilities.LogException("Write", exc, EventLogEntryType.Warning);
                    }
                }
                else
                {
                    string tempResponse = _html.ToString();
                    byte[] data = Encoding.UTF8.GetBytes(tempResponse);
                    _responseStream.Write(data, 0, data.Length);
                }
            }

            _responseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin direction)
        {
            return _responseStream.Seek(offset, direction);
        }

        public override void SetLength(long length)
        {
            _responseStream.SetLength(length);
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
