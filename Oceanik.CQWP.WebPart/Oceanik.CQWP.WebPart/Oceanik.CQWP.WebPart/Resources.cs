namespace Oceanik.CQWP.WebPart
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Threading;

    using Microsoft.SharePoint.Publishing.Internal;
    using Microsoft.SharePoint.Utilities;

    internal static class Resources
    {
        // Fields
        private static readonly Assembly ResourceAssembly = Assembly.Load("Microsoft.SharePoint.Publishing.intl, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
        private static readonly Assembly ResourceAssemblyDm = Assembly.Load("Microsoft.Office.DocumentManagement.intl, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
        private static readonly ResourceManager ResourceBrandStringManager = new ResourceManager("Microsoft.SharePoint.Publishing.BrandStrings", ResourceAssembly);
        private static readonly ResourceManager ResourceManager = new ResourceManager("Microsoft.SharePoint.Publishing.Strings", ResourceAssembly);
        private static readonly ResourceManager ResourceManagerDm = new ResourceManager("Microsoft.Office.DocumentManagement.Intl", ResourceAssemblyDm);

        // Methods
        internal static string GetBrandString(string resourceName)
        {
            return ResourceBrandStringManager.GetString(resourceName, Thread.CurrentThread.CurrentUICulture);
        }

        internal static string GetBrandStringEx(string resourceName, CultureInfo culture)
        {
            return ResourceBrandStringManager.GetString(resourceName, culture);
        }

        internal static string GetDocumentManagementString(string resourceName)
        {
            return ResourceManagerDm.GetString(resourceName, Thread.CurrentThread.CurrentUICulture);
        }

        internal static string GetFormattedBrandString(string resourceName, params object[] args)
        {
            return string.Format(Thread.CurrentThread.CurrentUICulture, GetBrandString(resourceName), args);
        }

        internal static string GetFormattedString(string resourceName, params object[] args)
        {
            return string.Format(Thread.CurrentThread.CurrentUICulture, GetString(resourceName), args);
        }

        internal static string GetFormattedStringEx(string resourceName, CultureInfo culture, params object[] args)
        {
            return string.Format(culture, GetStringEx(resourceName, culture), args);
        }

        internal static string GetString(string resourceName)
        {
            return ResourceManager.GetString(resourceName, Thread.CurrentThread.CurrentUICulture);
        }

        internal static string GetString(ResourceFileType fileType, string shortResourceName)
        {
            return GetString(fileType, shortResourceName, true);
        }

        internal static string GetString(ResourceFileType fileType, string shortResourceName, bool useUiCulture)
        {
            if (useUiCulture)
            {
                return ServerResources.GetString(fileType, shortResourceName, CultureInfo.CurrentUICulture);
            }

            return ServerResources.GetString(fileType, shortResourceName, CultureInfo.CurrentCulture);
        }

        internal static string GetStringEx(ResourceFileType fileType, string fullResourceName)
        {
            switch (fileType)
            {
                case ResourceFileType.CentralAdmin:
                case ResourceFileType.SiteAdmin:
                case ResourceFileType.Templates:
                    return SPUtility.GetLocalizedString(fullResourceName, null, (uint)CultureInfo.CurrentUICulture.LCID);

                case ResourceFileType.Satellite:
                    return GetString(fullResourceName);
            }

            throw new ArgumentOutOfRangeException("fileType");
        }

        internal static string GetStringEx(string resourceName, CultureInfo culture)
        {
            return ResourceManager.GetString(resourceName, culture);
        }

        internal static string GetStringFromAssembly(string assemblyName, string baseName, string stringId)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            if (executingAssembly != null)
            {
                AssemblyName name = executingAssembly.GetName();
                if (name != null)
                {
                    var assemblyRef = new AssemblyName { Name = assemblyName, Version = name.Version };

                    assemblyRef.SetPublicKey(name.GetPublicKey());
                    assemblyRef.CultureInfo = CultureInfo.InvariantCulture;
                    Assembly assembly = Assembly.Load(assemblyRef);
                    if (assembly != null)
                    {
                        var manager = new ResourceManager(baseName, assembly);
                        if (manager != null)
                        {
                            return manager.GetString(stringId);
                        }
                    }
                }
            }

            return null;
        }
    }
}