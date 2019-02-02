using System;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Constructors

        public ViewModelBase(
            IEventAggregator eventAggregator, 
            IBusyService busyService,
            IUnityContainer container)
        {
            this.EventAggregator = eventAggregator;
            this.BusyService = busyService;
            this.Container = container;
            //this.EventsToLoad = 0; // default
        }

        #endregion

        #region Properties

        protected IEventAggregator EventAggregator { get; set; }
        protected IBusyService BusyService { get; set; }
        protected IUnityContainer Container { get; set; }

        private int loadingEventCounter = 0;

        private int eventsToLoad;
        protected int EventsToLoad
        {
            get { return eventsToLoad; }
            set
            {
                eventsToLoad = value;
                this.HideBusyIfAllEventsAreLoaded();
            }
        }

        #endregion

        public delegate void EditStateChangedEventHandler(object sender, bool isEditing);
        public event EditStateChangedEventHandler EditStateChanged;
        protected void FireEditStateChanged(bool changed)
        {
            var handler = EditStateChanged;
            if (handler != null)
                handler(this, changed);
        }

        #region Event Declarations

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Protected Methods

        protected void FirePropertyChanged(string propertyname)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyname));
        }

        protected void HideBusyIfAllEventsAreLoaded()
        {
            this.HideBusyIfAllEventsAreLoaded(false);
        }

        protected void HideBusyIfAllEventsAreLoaded(bool incrementEventCounter)
        {
            if (this.EventsToLoad == 0 && !this.BusyService.IsBusy) return;// Do not hide if there was nothing going on. This was just an initialization

            if (incrementEventCounter) loadingEventCounter++;
            if (loadingEventCounter >= EventsToLoad)
            {
                // hide busy view and rest the counter
                loadingEventCounter = 0;
                this.BusyService.HideBusy();
            }
        }

        #endregion
    }
}