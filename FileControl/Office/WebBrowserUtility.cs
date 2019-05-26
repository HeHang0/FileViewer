using System.Windows;
using System.Windows.Controls;

namespace FileViewer.FileControl
{
    public static class WebBrowserUtility
    {
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserUtility), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = o as WebBrowser;
            var html = (e.NewValue as string) ?? "";
            if (html.Length > 0)
            {
                var index = html.IndexOf("<body ");
                browser?.NavigateToString(html.Insert(index+6, " oncontextmenu = 'self.event.returnValue=false' "));
            }
        }
    }
}
