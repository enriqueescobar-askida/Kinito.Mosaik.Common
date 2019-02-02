// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RawResponse.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the RawResponse type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace Alphamosaik.Oceanik.Caching
{
    [Serializable]
    internal class RawResponse : ISerializable
    {
        private readonly ArrayList _buffers;
        private readonly bool _hasSubstBlocks;
        private readonly NameValueCollection _headers;
        private readonly int _statusCode;
        private readonly string _statusDescr;
        private readonly DateTime _created;

        internal RawResponse(int statusCode, string statusDescription, NameValueCollection headers, ArrayList buffers, bool hasSubstBlocks)
        {
            _statusCode = statusCode;
            _statusDescr = statusDescription;
            _headers = headers;
            _buffers = GetInternalBuffer(buffers);
            _hasSubstBlocks = hasSubstBlocks;
            _created = DateTime.Now;
        }

        internal RawResponse(SerializationInfo info, StreamingContext context)
        {
            _hasSubstBlocks = info.GetBoolean("hasSubstBlocks");
            _statusCode = info.GetInt32("statusCode");
            _statusDescr = info.GetString("statusDescr");
            _created = info.GetDateTime("_created");
            _headers = (NameValueCollection)info.GetValue("headers", typeof(NameValueCollection));
            _buffers = (ArrayList)info.GetValue("buffers", typeof(ArrayList));

            /*foreach (var buffer in buffers)
            {
                if (buffer.GetType() == typeof(ResponseSubstBlockElement))
                {
                    
                }
                else if (buffer.GetType() == typeof(ResponseBufferElement))
                {
                    
                }
            }*/
        }

        public DateTime Created
        {
            get { return _created; }
        }

        internal ArrayList Buffers
        {
            get
            {
                return _buffers;
            }
        }

        internal bool HasSubstBlocks
        {
            get
            {
                return _hasSubstBlocks;
            }
        }

        internal NameValueCollection Headers
        {
            get
            {
                return _headers;
            }
        }

        internal int StatusCode
        {
            get
            {
                return _statusCode;
            }
        }

        internal string StatusDescription
        {
            get
            {
                return _statusDescr;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("hasSubstBlocks", _hasSubstBlocks);
            info.AddValue("statusCode", _statusCode);
            info.AddValue("statusDescr", _statusDescr);
            info.AddValue("_created", _created);
            info.AddValue("headers", _headers);
            info.AddValue("buffers", _buffers);

            /*var transportBuffer = new ArrayList(_buffers.Count);

            foreach (var buffer in _buffers)
            {
                Type type = buffer.GetType();

                if (type.Name.IndexOf("HttpSubstBlockResponseElement") != -1)
                {
                    FieldInfo callbackInfo = buffer.GetType().GetField("_callback", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (callbackInfo != null)
                    {
                        var callback = (HttpResponseSubstitutionCallback)callbackInfo.GetValue(buffer);

                        if (callback != null)
                        {
                            var responseSubstBlockElement = new ResponseSubstBlockElement(callback, Encoding.UTF8);
                            transportBuffer.Add(responseSubstBlockElement);
                        }
                    }
                }
                else if (type.Name.IndexOf("HttpResponseBufferElement") != -1 || type.Name.IndexOf("HttpResponseUnmanagedBufferElement") != -1)
                {
                    MethodInfo methodInfo = buffer.GetType().GetMethod("GetBytes", BindingFlags.Public |
                                                                       BindingFlags.Instance |
                                                                       BindingFlags.InvokeMethod);

                    if (methodInfo != null)
                    {
                        var bytes = (byte[])methodInfo.Invoke(buffer, null);

                        MethodInfo sizeInfo = buffer.GetType().GetMethod("GetSize", BindingFlags.Public |
                                                                       BindingFlags.Instance |
                                                                       BindingFlags.InvokeMethod);

                        if (sizeInfo != null)
                        {
                            var size = (long)sizeInfo.Invoke(buffer, null);
                            var responseBufferElement = new ResponseBufferElement(bytes, (int)size);

                            transportBuffer.Add(responseBufferElement);
                        }
                    }
                }
            }

            info.AddValue("buffers", transportBuffer);*/
        }

        internal ArrayList GetInternalBuffer(ArrayList buffers)
        {
            var transportBuffer = new ArrayList(buffers.Count);

            foreach (var buffer in buffers)
            {
                Type type = buffer.GetType();

                if (type.Name.IndexOf("HttpSubstBlockResponseElement") != -1)
                {
                    FieldInfo callbackInfo = buffer.GetType().GetField("_callback", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (callbackInfo != null)
                    {
                        var callback = (HttpResponseSubstitutionCallback)callbackInfo.GetValue(buffer);

                        if (callback != null)
                        {
                            var responseSubstBlockElement = new ResponseSubstBlockElement(callback, Encoding.UTF8);
                            transportBuffer.Add(responseSubstBlockElement);
                        }
                    }
                }
                else if (type.Name.IndexOf("HttpResponseBufferElement") != -1 || type.Name.IndexOf("HttpResponseUnmanagedBufferElement") != -1)
                {
                    MethodInfo methodInfo = buffer.GetType().GetMethod("System.Web.IHttpResponseElement.GetBytes", BindingFlags.NonPublic | BindingFlags.Instance |
                                                                       BindingFlags.InvokeMethod);

                    foreach (MethodInfo method in buffer.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                    {
                        Trace.WriteLine(method.Name);
                    }

                    if (methodInfo != null)
                    {
                        var bytes = (byte[])methodInfo.Invoke(buffer, null);

                        MethodInfo sizeInfo = buffer.GetType().GetMethod("System.Web.IHttpResponseElement.GetSize", BindingFlags.NonPublic |
                                                                       BindingFlags.Instance |
                                                                       BindingFlags.InvokeMethod);

                        if (sizeInfo != null)
                        {
                            var size = (long)sizeInfo.Invoke(buffer, null);
                            var responseBufferElement = new ResponseBufferElement(bytes, (int)size);

                            transportBuffer.Add(responseBufferElement);
                        }
                    }
                }
            }

            return transportBuffer;
        }
    }
}
