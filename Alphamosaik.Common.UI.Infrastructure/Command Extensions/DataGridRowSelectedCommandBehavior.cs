using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class DataGridRowSelectedCommandBehavior : CommandBehaviorBase<DataGrid>
    {
        public DataGridRowSelectedCommandBehavior(DataGrid selectableObject)
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