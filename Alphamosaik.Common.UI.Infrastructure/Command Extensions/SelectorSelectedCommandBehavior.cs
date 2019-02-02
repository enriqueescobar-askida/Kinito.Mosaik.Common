using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class SelectorSelectedCommandBehavior : CommandBehaviorBase<Selector>
    {
        public SelectorSelectedCommandBehavior(Selector selectableObject)
            : base(selectableObject)
        {
            selectableObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CommandParameter = this.TargetObject.SelectedItem;
            ExecuteCommand();
        }
    }
}
