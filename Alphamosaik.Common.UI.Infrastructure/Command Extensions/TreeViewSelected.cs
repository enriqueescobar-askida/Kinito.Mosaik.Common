using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public static class TreeViewSelected
    {
        private static readonly DependencyProperty SelectedCommandBehaviorProperty = DependencyProperty.RegisterAttached(
            "SelectedCommandBehavior",
            typeof(TreeViewSelectedCommandBehavior),
            typeof(TreeViewSelected),
            null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(TreeViewSelected),
            new PropertyMetadata(OnSetCommandCallback));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only works for TreeView")]
        public static void SetCommand(TreeView TreeView, ICommand command)
        {
            TreeView.SetValue(CommandProperty, command);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only works for TreeView")]
        public static ICommand GetCommand(TreeView TreeView)
        {
            return TreeView.GetValue(CommandProperty) as ICommand;
        }

        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var TreeView = dependencyObject as TreeView;
            if (TreeView != null)
                GetOrCreateBehavior(TreeView).Command = e.NewValue as ICommand;
        }

        private static TreeViewSelectedCommandBehavior GetOrCreateBehavior(TreeView TreeView)
        {
            var behavior = TreeView.GetValue(SelectedCommandBehaviorProperty) as TreeViewSelectedCommandBehavior;
            if (behavior == null)
            {
                behavior = new TreeViewSelectedCommandBehavior(TreeView);
                TreeView.SetValue(SelectedCommandBehaviorProperty, behavior);
            }

            return behavior;
        }
    }
}