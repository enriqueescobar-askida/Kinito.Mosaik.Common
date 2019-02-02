// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DumpObject.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DumpObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace Alphamosaik.Common.Library
{
    public class DumpObject
    {
        public static void Dump(object o, StringBuilder sb, BindingFlags flags)
        {
            try
            {
                Dump(o, sb, true, flags);
            }
            catch
            {
            }
            
        }

        private static void Dump(object o, StringBuilder sb, bool ulli, BindingFlags flags)
        {
            if (ulli)
                sb.Append("<ul>");

            if (o is string || o is int || o is long || o is double)
            {
                if (ulli)
                    sb.Append("<li>");

                sb.Append(o.ToString());

                if (ulli)
                    sb.Append("</li>");
            }
            else
            {
                Type t = o.GetType();
                foreach (PropertyInfo p in t.GetProperties(flags | BindingFlags.Instance))
                {
                    sb.Append("<li><b>" + p.Name + ":</b> ");
                    object val = null;

                    try
                    {
                        val = p.GetValue(o, null);
                    }
                    catch
                    {
                    }

                    if (val is string || val is int || val is long || val is double)
                        sb.Append(val);
                    else if (val != null)
                    {
                        var arr = val as Array;
                        if (arr == null)
                        {
                            var nv = val as NameValueCollection;
                            if (nv == null)
                            {
                                var ie = val as IEnumerable;
                                if (ie == null)
                                    sb.Append(val.ToString());
                                else
                                {
                                    try
                                    {
                                        foreach (object oo in ie)
                                            Dump(oo, sb, flags);
                                    }
                                    catch
                                    {
                                    }
                                    
                                }
                            }
                            else
                            {
                                sb.Append("<ul>");

                                try
                                {
                                    foreach (string key in nv.AllKeys)
                                    {
                                        sb.AppendFormat("<li>{0} = ", key);
                                        Dump(nv[key], sb, false, flags);
                                        sb.Append("</li>");
                                    }
                                }
                                catch
                                {
                                }
                                

                                sb.Append("</ul>");
                            }
                        }
                        else
                        {
                            try
                            {
                                foreach (object oo in arr)
                                    Dump(oo, sb, flags);
                            }
                            catch
                            {
                            }
                            
                        }
                    }
                    else
                    {
                        sb.Append("<i>null</i>");
                    }

                    sb.Append("</li>");
                }
            }

            if (ulli)
                sb.Append("</ul>");
        }
    }
}
