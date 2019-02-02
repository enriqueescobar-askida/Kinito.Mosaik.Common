using System.Windows;
using System.Windows.Input;
using DevExpress.AgMenu;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public static class MenuClicked
    {
        private static readonly DependencyProperty MenuClickedCommandBehaviorProperty = DependencyProperty.RegisterAttached(
            "MenuClickedCommandBehavior",
            typeof(MenuClickedCommandBehavior),
            typeof(MenuClicked),
            null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(MenuClicked),
            new PropertyMetadata(OnSetCommandCallback));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only works for AgMenu")]
        public static void SetCommand(AgMenuBase menu, ICommand command)
        {
            menu.SetValue(CommandProperty, command);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only works for AgMenu")]
        public static ICommand GetCommand(AgMenuBase menu )
        {
            return menu.GetValue(CommandProperty) as ICommand;
        }

        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var menu = dependencyObject as AgMenuBase;
            if (menu != null)
                GetOrCreateBehavior(menu).Command = e.NewValue as ICommand;
        }

        private static MenuClickedCommandBehavior GetOrCreateBehavior(AgMenuBase menu)
        {
            var behavior = menu.GetValue(MenuClickedCommandBehaviorProperty) as MenuClickedCommandBehavior;
            if (behavior == null)
            {
                behavior = new MenuClickedCommandBehavior(menu);
                menu.SetValue(MenuClickedCommandBehaviorProperty, behavior);
            }

            return behavior;
        }
    }
}