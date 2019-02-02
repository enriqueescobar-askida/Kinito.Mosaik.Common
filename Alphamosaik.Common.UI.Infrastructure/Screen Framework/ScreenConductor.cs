using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Regions;
using Alphamosaik.Common.UI.Infrastructure.Constants;

namespace Alphamosaik.Common.UI.Infrastructure
{
    //public class ScreenCollection
    //{
    //    protected IDictionary<ScreenKeyType, IScreen> Screens { get; set; }

    //    public bool ScreenExists(ScreenKeyType key)
    //    {
    //        return this.Screens.ContainsKey(key);
    //    }
        
    //    public void AddScreen(ScreenKeyType key, IScreen screen, string regionName)
    //    {
    //        this.Screens.Add(key, screen);
    //    }
    //}

    //public class ScreenWrapper
    //{
    //    public ScreenKeyType ScreenKeyType { get; set; }
    //    public string RegionName { get; set; }
    //}

    public class ScreenConductor
    {
        #region Properties

        protected IUnityContainer Container { get; set; }
        protected IScreenFactoryRegistry ScreenFactoryRegistry { get; set; }
        protected IRegionManager RegionManager { get; set; }
        protected IEventAggregator EventAggregator { get; set; }
        protected IVisibilityService VisibilityService { get; set; }
        protected IDictionary<ScreenKeyType, IScreen> ScreenCollection { get; set; }

        #endregion

        #region Fields

        private ScreenKeyType activeScreenKey;
        private readonly IRegion mainRegion;
        private IRegion targetRegion; 

        #endregion

        #region Constructors

        public ScreenConductor(IUnityContainer container, 
            IScreenFactoryRegistry screenFactoryRegistry, 
            IEventAggregator eventAggregator, 
            IRegionManager regionManager, 
            IVisibilityService visibilityService)
        {
            this.activeScreenKey = ScreenKeyType.None;

            this.Container = container;
            this.ScreenFactoryRegistry = screenFactoryRegistry;
            this.EventAggregator = eventAggregator;
            this.RegionManager = regionManager;
            this.VisibilityService = visibilityService;

            this.ScreenCollection = new Dictionary<ScreenKeyType, IScreen>();
            mainRegion = this.RegionManager.Regions[RegionConstants.REGION_MAIN_AREA];
            SubscribeToEvents();
        }

        #endregion

        #region Private Methods

        private void SubscribeToEvents()
        {
            this.EventAggregator.GetEvent<ScreenActivateEvent>().Subscribe(
                    ActivateScreen,
                    ThreadOption.UIThread,
                    true,
                    IsNotificationRelevant);
        }

        private bool IsNotificationRelevant(ScreenEventArgs args)
        {
            //TODO: PAPA :-)
            return true;
        }

        private void ActivateScreen(ScreenEventArgs args)
        {
            // There is no such registered screen type. Possibly throw an exception here.
            if (!this.ScreenFactoryRegistry.HasFactory(args.ScreenKey)) return;

            // Check if an active screen exists. 
            if (this.ScreenCollection.ContainsKey(this.activeScreenKey))
            {
                // Get the currently active screen
                IScreen activeScreen = this.ScreenCollection[this.activeScreenKey];
                // Check if we can leave
                if (activeScreen.CanLeave())
                {
                    IScreen screen = this.ScreenCollection[this.activeScreenKey];
                    this.VisibilityService.LeaveViewAnimation(screen.View, () =>
                                                                               {
                                                                                   mainRegion.Deactivate(screen.View);
                                                                                   mainRegion.Remove(screen.View);
                                                                                   IScreen newScreen = EnsureScreenExists(args.ScreenKey, args.ScreenSubject, args.RegionName);
                                                                                   ShowScreen(args.ScreenKey, args.RegionName);
                                                                               });
                }
                else
                {
                    activeScreen.CleanUp(); //TODO: implement CleanUp
                }
            }
            else
            {
                // no active screen exists
                IScreen newScreen = EnsureScreenExists(args.ScreenKey, args.ScreenSubject, args.RegionName);
                ShowScreen(args.ScreenKey, args.RegionName);
            }
        }

        private IScreen EnsureScreenExists(ScreenKeyType screenKey, object screenSubject, string regionName)
        {
            IScreen screen = null;
            // use the screen type to see if the screen exists in the collection
            if (ScreenCollection.ContainsKey(screenKey))
            {
                screen = ScreenCollection[screenKey];
            }
            else // if it does not, then use the screen type to get the factory that is made for creating that type of screen and make it, add to collection
            {
                if (this.ScreenFactoryRegistry.HasFactory(screenKey))
                {
                    screen = this.ScreenFactoryRegistry.Get(screenKey).CreateScreen(screenSubject);
                    ScreenCollection.Add(screenKey, screen);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Screen Key not found");
                }
            }
            return screen;
        }

        private void ShowScreen(ScreenKeyType screenKey, string regionName)
        {
            if (!this.ScreenCollection.ContainsKey(screenKey)) return;

            IScreen screen = this.ScreenCollection[screenKey];
            if (regionName != null && regionName != RegionConstants.REGION_MAIN_AREA)
            {
                targetRegion = this.RegionManager.Regions[regionName];
                targetRegion.Add(screen.View);
                targetRegion.Activate(screen.View);
            }
            else
            {
                this.activeScreenKey = screenKey;
                mainRegion.Add(screen.View);
                mainRegion.Activate(screen.View);
            }
            this.VisibilityService.EnterViewAnimation(screen.View);
        }

        #endregion
    }
}