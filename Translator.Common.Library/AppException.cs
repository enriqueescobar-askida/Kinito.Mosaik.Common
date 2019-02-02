// ----------------------------------------------------------public ----------------------------------------------------------
// <copyright file="AppException.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the AppException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Translator.Common.Library
{
    public static class AppException
    {
        public static string ExceptionMessage(Exception exception, string functionName, string className)
        {
            return "Error in Translator.Common.Library" + "." + className + "." + functionName + ": " + exception.Message;
        }
    }
}