using System.Collections.Generic;
using Microsoft.Practices.Composite.Regions;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public static class RegionExtensions
    {
        public static void RemoveAll(this IRegion region)
        {
            List<object> views = new List<object>();
            views.AddRange(region.Views);
            foreach (var view in views)
            {
                //region.Deactivate(view);
                region.Remove(view);
            }
        }

        public static void AddAndActivateIfNotExists(this IRegion region, object view)
        {
            if (!region.Views.Contains(view))
            {
                region.Add(view);
                region.Activate(view);
            }
        }
    }
}