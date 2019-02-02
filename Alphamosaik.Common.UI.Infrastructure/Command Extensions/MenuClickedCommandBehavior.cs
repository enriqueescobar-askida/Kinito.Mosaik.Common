using DevExpress.AgMenu;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class MenuClickedCommandBehavior : CommandBehaviorBase<AgMenuBase>
    {
        public MenuClickedCommandBehavior(AgMenuBase menuItem)
            : base(menuItem)
        {
            menuItem.ItemClick += OnMenuItemClicked;
        }

        private void OnMenuItemClicked(object sender, AgMenuEventEventArgs e)
        {
            this.CommandParameter = this.TargetObject.SelectedItem.Item;
            ExecuteCommand();
        }
    }
}