using System;
using System.Windows;
using System.Windows.Data;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class DataContextProxy : FrameworkElement
    {
        public DataContextProxy()
        {
            this.Loaded += DataContextProxy_Loaded;
        }

        public string BindingPropertyName { get; set; }
        public BindingMode BindingMode { get; set; }

        void DataContextProxy_Loaded(object sender, RoutedEventArgs e)
        {
            var binding = new Binding { Source = this.DataContext, Mode = BindingMode };
            if (!String.IsNullOrEmpty(BindingPropertyName))
                binding.Path = new PropertyPath(BindingPropertyName);
            this.SetBinding(DataContextProxy.DataSourceProperty, binding);
        }

        public Object DataSource
        {
            get { return (Object)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(Object), typeof(DataContextProxy), null);
    }
}