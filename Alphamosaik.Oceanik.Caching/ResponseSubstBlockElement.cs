// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseSubstBlockElement.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ResponseSubstBlockElement type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace Alphamosaik.Oceanik.Caching
{
    [Serializable]
    internal class ResponseSubstBlockElement : IResponseElement,  ISerializable
    {
        private readonly HttpResponseSubstitutionCallback _callback;
        private readonly IResponseElement _firstSubstitution;

        public ResponseSubstBlockElement(HttpResponseSubstitutionCallback callback, Encoding encoding)
        {
            _callback = callback;
            _firstSubstitution = Substitute(encoding);
        }

        internal ResponseSubstBlockElement(SerializationInfo info, StreamingContext context)
        {
            // Simply retrieve the action if it is serializable
            if (info.GetBoolean("isSerializable"))
                _callback = (HttpResponseSubstitutionCallback)info.GetValue("callback", typeof(HttpResponseSubstitutionCallback));

            // Otherwise, recreate the action based on its serialized components
            else
            {
                // Retrieve the serialized method reference
                var method = (MethodInfo)info.GetValue("method", typeof(MethodInfo));

                // Create an instance of the anonymous delegate class
                object target = Activator.CreateInstance(method.DeclaringType, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

                // Initialize the fields of the anonymous instance
                foreach (FieldInfo field in method.DeclaringType.GetFields())
                    field.SetValue(target, info.GetValue(field.Name, field.FieldType));

                // Recreate the action delegate
                _callback = (HttpResponseSubstitutionCallback)Delegate.CreateDelegate(typeof(HttpResponseSubstitutionCallback), target, method.Name);
            }
        }

        public HttpResponseSubstitutionCallback Callback
        {
            get { return _callback; }
        }

        public byte[] GetBytes()
        {
            return _firstSubstitution.GetBytes();
        }

        public long GetSize()
        {
            return _firstSubstitution.GetSize();
        }

        public IResponseElement Substitute(Encoding e)
        {
            string s = Callback(HttpContext.Current);
            byte[] bytes = e.GetBytes(s);
            return new ResponseBufferElement(bytes, bytes.Length);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize the action delegate directly if the target is serializable
            if (Callback.Target == null || Callback.Target.GetType().GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
            {
                info.AddValue("isSerializable", true);
                info.AddValue("callback", Callback);
            }
            else
            {
                // Otherwise, serialize information necessary to recreate the action delegate
                info.AddValue("isSerializable", false);
                info.AddValue("method", Callback.Method);

                foreach (FieldInfo field in Callback.Method.DeclaringType.GetFields())
                    info.AddValue(field.Name, field.GetValue(Callback.Target));
            }
        }
    }
}
