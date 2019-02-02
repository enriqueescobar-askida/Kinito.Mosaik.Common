using System.Windows.Controls;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public interface IScreen
    {
        bool CanEnter();
        bool CanLeave();
        void CleanUp();
        UserControl View { get; set; }
    }
}