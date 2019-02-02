using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class DefaultButtonKey
    {
        #region Properties

        private Button DefaultButton { get; set; }
        private ButtonAutomationPeer peer { get; set; }

        #endregion

        #region Attach Method (private)

        private void Attach(DependencyObject source)
        {
            if (source is Button)
            {
                this.DefaultButton = (source as Button);
                peer = new ButtonAutomationPeer(source as Button);
            }
            else if (source is Control)
            {
                var ctrl = (source as Control);
                ctrl.KeyUp += OnKeyUp;
            }  
            //else if (source is TextBox)
            //    (source as TextBox).KeyUp += OnKeyUp;
            //else if (source is AutoCompleteBox)
            //    (source as AutoCompleteBox).KeyUp += OnKeyUp;
            //else if (source is ComboBox)
            //    (source as ComboBox).KeyUp += OnKeyUp;
            //else if (source is PasswordBox)
            //    (source as PasswordBox).KeyUp += OnKeyUp;
            //else if (source is CheckBox)
            //    (source as CheckBox).KeyUp += OnKeyUp;
        }

        #endregion

        #region OnKeyUp Event Handler

        private void OnKeyUp(object sender, KeyEventArgs arg)
        {
            if (arg.Key == Key.Enter && peer != null)
            {
                this.DefaultButton.Focus();
                if (!this.DefaultButton.IsEnabled) return;

                ((IInvokeProvider)peer).Invoke();
                ((Control)sender).Focus();
            }
        }

        #endregion

        #region Dependency Property for Default Key

        public static DefaultButtonKey GetDefaultKey(DependencyObject obj)
        {
            return (DefaultButtonKey)obj.GetValue(DefaultKeyProperty);
        }

        public static void SetDefaultKey(DependencyObject obj, DefaultButtonKey value)
        {
            obj.SetValue(DefaultKeyProperty, value);
        }

        public static readonly DependencyProperty DefaultKeyProperty =
            DependencyProperty.RegisterAttached("DefaultKey", typeof(DefaultButtonKey), typeof(DefaultButtonKey), new PropertyMetadata(OnKeyAttach));

        private static void OnKeyAttach(DependencyObject source, DependencyPropertyChangedEventArgs prop)
        {
            var key = prop.NewValue as DefaultButtonKey;
            key.Attach(source);
        }

        #endregion
    }
}



