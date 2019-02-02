using System.Windows.Controls;
using Microsoft.Practices.Unity;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public abstract class ScreenBase : IScreen
    {
        protected IUnityContainer Container { get; set; }

        protected ScreenBase(IUnityContainer container)
        {
            this.Container = container;
        }

        #region Implementation of IScreen

        public abstract bool CanEnter();
        public abstract bool CanLeave();
        public abstract void CleanUp();
        public abstract UserControl View { get; set; }

        #endregion
    }
}