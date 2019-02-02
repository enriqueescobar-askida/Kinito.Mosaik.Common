using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Alphamosaik.Common.UI.Infrastructure
{
    /// <summary>
    /// This class applies to the DataGridRowSelectedCommandBehavior for DataGrid controls
    /// </summary>
    public static class DataGridRowSelected
    {
        private static readonly DependencyProperty DataGridRowSelectedCommandBehaviorProperty = DependencyProperty.RegisterAttached(
            "SelectedCommandBehavior",
            typeof(DataGridRowSelectedCommandBehavior),
            typeof(DataGridRowSelected),
            null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(DataGridRowSelected),
            new PropertyMetadata(OnSetCommandCallback));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only works for DataGrid")]
        public static void SetCommand(DataGrid dataGrid, ICommand command)
        {
            dataGrid.SetValue(CommandProperty, command);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only works for DataGrid")]
        public static ICommand GetCommand(DataGrid dataGrid)
        {
            return dataGrid.GetValue(CommandProperty) as ICommand;
        }

        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid != null)
                GetOrCreateBehavior(dataGrid).Command = e.NewValue as ICommand;
        }

        private static DataGridRowSelectedCommandBehavior GetOrCreateBehavior(DataGrid dataGrid)
        {
            var behavior = dataGrid.GetValue(DataGridRowSelectedCommandBehaviorProperty) as DataGridRowSelectedCommandBehavior;
            if (behavior == null)
            {
                behavior = new DataGridRowSelectedCommandBehavior(dataGrid);
                dataGrid.SetValue(DataGridRowSelectedCommandBehaviorProperty, behavior);
            }

            return behavior;
        }
    }
}