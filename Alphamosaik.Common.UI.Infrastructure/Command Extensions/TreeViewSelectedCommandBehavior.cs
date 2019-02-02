using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class TreeViewSelectedCommandBehavior : CommandBehaviorBase<TreeView>
    {
        public TreeViewSelectedCommandBehavior(TreeView selectableObject)
            : base(selectableObject)
        {
            selectableObject.SelectedItemChanged += SelectedItemChanged;
        }

        private void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.CommandParameter = this.TargetObject.SelectedItem;
            ExecuteCommand();
        }
    }
}