using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Regions;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class GridRegionAdapter : RegionAdapterBase<Grid>
    {
        public GridRegionAdapter(IRegionBehaviorFactory behaviorFactory) :
            base(behaviorFactory)
        {

        }

        protected override void Adapt(IRegion region, Grid regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
                                                  {
                                                      //Add
                                                      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                                                          foreach (FrameworkElement element in e.NewItems)
                                                              regionTarget.Children.Add(element);
                    

                                                      //Removal
                                                      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                                                          foreach (FrameworkElement element in e.OldItems)
                                                              if (regionTarget.Children.Contains(element))
                                                                  regionTarget.Children.Remove(element);
                                                  };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

    }
}