using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Composite.Regions;
using Alphamosaik.Common.UI.Infrastructure.Constants;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class BusyService : IBusyService
    {
        #region Properties

        protected IRegionManager RegionManager { get; set; }
        private FlowerLoadingControl BusyControl { get; set; }
        public bool IsBusy { get; set; }

        #endregion

        #region Constructors

        public BusyService(IRegionManager regionManager)
        {
            this.RegionManager = regionManager;
            this.IsBusy = false;

            Color color = new Color { A = 255, R = 255, B = 0, G = 255 }; // yellow
            //Color color = new Color { A = 255, R = 0, B = 0, G = 255 }; // green
            //Color color = new Color { A = 255, R = 16, B = 128, G = 80}; // blue shade
            BusyControl = new FlowerLoadingControl { Visibility = Visibility.Collapsed, PetalBrush = new SolidColorBrush(color), Caption = "Loading, please wait ..." };

            ShowInRegion();
        }

        #endregion

        #region Public Methods

        public void ShowBusy()
        {
            this.IsBusy = true;
            ShowInRegion();
            BusyControl.Visibility = Visibility.Visible;
        }

        public void HideBusy()
        {
            this.IsBusy = false;
            BusyControl.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Private Methods

        private void ShowInRegion()
        {
            IRegion region = this.RegionManager.Regions[RegionConstants.REGION_BUSY];
            region.RemoveAll();
            region.AddAndActivateIfNotExists(BusyControl);
        }
        
        #endregion
    }
}