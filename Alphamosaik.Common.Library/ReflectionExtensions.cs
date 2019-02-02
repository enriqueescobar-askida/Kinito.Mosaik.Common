// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ReflectionExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library
{
    using System;
    using System.Reflection;

    public static class ReflectionExtensions
    {  
        public static FieldInfo GetBaseField(this Type type, string name, BindingFlags flags)
        {
            if (type == typeof(object))
            {
                return null;
            }

            var field = type.GetField(name, flags);

            if (field == null && type.BaseType != null)
            {
                // in order to avoid duplicates, force BindingFlags.DeclaredOnly 
                return type.BaseType.GetBaseField(name, flags | BindingFlags.DeclaredOnly); 
            }

            return field;
        }
    }
}