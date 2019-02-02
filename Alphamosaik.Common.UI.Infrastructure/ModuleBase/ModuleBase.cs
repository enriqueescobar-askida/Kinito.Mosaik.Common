using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public abstract class ModuleBase : IModule
    {
        #region Properties

        protected IUnityContainer Container { get; set; }
        protected IScreenFactoryRegistry ScreenFactoryRegistry { get; set; }

        #endregion

        #region Constructors

        public ModuleBase(IUnityContainer container, IScreenFactoryRegistry screenFactoryRegistry)
        {
            this.Container = container;
            this.ScreenFactoryRegistry = screenFactoryRegistry;
        }

        #endregion

        #region Implementation of IModule

        public virtual void Initialize()
        {
            RegisterViewsAndServices();
            RegisterScreenFactories();
        }

        protected abstract void RegisterScreenFactories();

        protected abstract void RegisterViewsAndServices();

        #endregion
    }
}