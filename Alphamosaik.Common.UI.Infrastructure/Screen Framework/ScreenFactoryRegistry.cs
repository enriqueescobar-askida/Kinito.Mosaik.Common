using System;
using Microsoft.Practices.Unity;
using System.Collections.Generic;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class ScreenFactoryRegistry : IScreenFactoryRegistry
    {
        #region Properties

        protected IUnityContainer Container { get; set; }
        protected IDictionary<ScreenKeyType, Type> ScreenFactoryCollection { get; set; }

        #endregion

        #region Constructors

        public ScreenFactoryRegistry(IUnityContainer container)
        {
            this.Container = container;
            this.ScreenFactoryCollection = new Dictionary<ScreenKeyType, Type>();
        }

        #endregion

        #region Get

        public IScreenFactory Get(ScreenKeyType screenType)
        {
            IScreenFactory screenFactory = null;
            if (this.HasFactory(screenType))
            {
                Type registeredScreenFactory = ScreenFactoryCollection[screenType];
                screenFactory = (IScreenFactory)Container.Resolve(registeredScreenFactory);
            }
            return screenFactory;
        }

        #endregion

        #region Register

        public void Register(ScreenKeyType screenType, Type registeredScreenFactoryType)
        {
            if (!HasFactory(screenType))
                this.ScreenFactoryCollection.Add(screenType, registeredScreenFactoryType);
        }

        #endregion

        #region HasFactory

        public bool HasFactory(ScreenKeyType screenType)
        {
            return (this.ScreenFactoryCollection.ContainsKey(screenType));
        }

        #endregion
    }
}
