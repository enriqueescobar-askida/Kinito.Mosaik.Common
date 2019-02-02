using System.Windows;
using System.Windows.Controls;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class EditableTreeViewItem : ContentControl
    {
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableTreeViewItem), new PropertyMetadata((bool)false, new PropertyChangedCallback(OnIsEditingChanged)));

        private object content;

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EditableTreeViewItem)d).OnIsEditingChanged(e);
        }

        protected virtual void OnIsEditingChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInEdit = (bool)e.NewValue;
            if (isInEdit)
            {
                content = Content;
                Content = EditorTemplate.LoadContent();
            }
            else
            {
                Content = content;
            }
            Focus();
        }

        public static readonly DependencyProperty EditorTemplateProperty = DependencyProperty.Register("EditorTemplate", typeof(DataTemplate), typeof(EditableTreeViewItem), new PropertyMetadata((DataTemplate)null));

        public DataTemplate EditorTemplate
        {
            get { return (DataTemplate)GetValue(EditorTemplateProperty); }
            set { SetValue(EditorTemplateProperty, value); }
        }

        public EditableTreeViewItem()
        {
        }
    }
}