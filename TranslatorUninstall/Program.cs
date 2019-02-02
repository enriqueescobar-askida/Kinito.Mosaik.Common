// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace TranslatorUninstall
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string[] varArguments = Environment.GetCommandLineArgs();

                foreach (string arg in varArguments)
                {
                    string[] varParameters = arg.Split('=');

                    if (varParameters[0].ToLower() == "/u")
                    {
                        string varProductCode = varParameters[1];
                        string varPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                        var varProcess = new Process
                                             {
                                                 StartInfo =
                                                     {
                                                         FileName = string.Concat(varPath, "\\msiexec.exe"),
                                                         Arguments = string.Concat("/x", varProductCode)
                                                     }
                                             };
                        varProcess.Start();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }
    }
}
