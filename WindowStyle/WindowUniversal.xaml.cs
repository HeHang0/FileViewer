using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer.WindowStyle
{
    public class UniversalWindowStyle
    {
        public static readonly DependencyProperty TitleBarProperty = DependencyProperty.RegisterAttached(
            "TitleBar", typeof(UniversalTitleBar), typeof(UniversalWindowStyle),
            new PropertyMetadata(new UniversalTitleBar(), OnTitleBarChanged));

        public static UniversalTitleBar GetTitleBar(DependencyObject element)
            => (UniversalTitleBar)element.GetValue(TitleBarProperty);

        public static void SetTitleBar(DependencyObject element, UniversalTitleBar value)
            => element.SetValue(TitleBarProperty, value);

        public static readonly DependencyProperty OpenCommondProperty = DependencyProperty.RegisterAttached(
            "OpenCommond", typeof(ICommand), typeof(UniversalWindowStyle),
            new PropertyMetadata(null, OnOpenCommondChanged));

        public static ICommand GetOpenCommond(DependencyObject element) => (ICommand)element.GetValue(OpenCommondProperty);

        public static void SetOpenCommond(DependencyObject element, ICommand value)
            => element.SetValue(OpenCommondProperty, value);

        public static readonly DependencyProperty OpenTextProperty = DependencyProperty.RegisterAttached(
            "OpenText", typeof(string), typeof(UniversalWindowStyle),
            new PropertyMetadata(null, OnOpenTextChanged));

        public static string GetOpenText(DependencyObject element) => (string)element.GetValue(OpenTextProperty);

        public static void SetOpenText(DependencyObject element, ICommand value)
            => element.SetValue(OpenTextProperty, value);

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(UniversalWindowStyle),
            new PropertyMetadata(Brushes.Black));

        public static Brush GetForeground(DependencyObject element) => (Brush)element.GetValue(ForegroundProperty);

        public static void SetForeground(DependencyObject element, Brush value)
            => element.SetValue(ForegroundProperty, value);

        public static readonly DependencyProperty TitleBarButtonStateProperty = DependencyProperty.RegisterAttached(
            "TitleBarButtonState", typeof(WindowState?), typeof(UniversalWindowStyle),
            new PropertyMetadata(null, OnButtonStateChanged));

        public static WindowState? GetTitleBarButtonState(DependencyObject element)
            => (WindowState?)element.GetValue(TitleBarButtonStateProperty);

        public static void SetTitleBarButtonState(DependencyObject element, WindowState? value)
            => element.SetValue(TitleBarButtonStateProperty, value);

        public static readonly DependencyProperty IsTitleBarCloseButtonProperty = DependencyProperty.RegisterAttached(
            "IsTitleBarCloseButton", typeof(bool), typeof(UniversalWindowStyle),
            new PropertyMetadata(false, OnIsCloseButtonChanged));

        public static bool GetIsTitleBarCloseButton(DependencyObject element)
            => (bool)element.GetValue(IsTitleBarCloseButtonProperty);

        public static void SetIsTitleBarCloseButton(DependencyObject element, bool value)
            => element.SetValue(IsTitleBarCloseButtonProperty, value);

        private static void OnTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is null) throw new NotSupportedException("TitleBar property should not be null.");
        }

        private static void OnButtonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;

            if (e.OldValue is WindowState)
            {
                button.Click -= StateButton_Click;
            }

            if (e.NewValue is WindowState)
            {
                button.Click -= StateButton_Click;
                button.Click += StateButton_Click;
            }
        }

        private static void OnOpenCommondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnOpenTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnIsCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;

            if (e.OldValue is true)
            {
                button.Click -= CloseButton_Click;
            }

            if (e.NewValue is true)
            {
                button.Click -= CloseButton_Click;
                button.Click += CloseButton_Click;
            }
        }

        private static void StateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (DependencyObject)sender;
            var window = Window.GetWindow(button);
            var state = GetTitleBarButtonState(button);
            if (window != null && state != null)
            {
                window.WindowState = state.Value;
            }
        }

        private static void CloseButton_Click(object sender, RoutedEventArgs e)
            => Window.GetWindow((DependencyObject)sender)?.Close();
    }

    public class UniversalTitleBar
    {
        public Color ForegroundColor { get; set; } = Colors.Black;
        public Color InactiveForegroundColor { get; set; } = Color.FromRgb(0x99, 0x99, 0x99);
        public Color ButtonHoverForeground { get; set; } = Colors.Black;
        public Color ButtonHoverBackground { get; set; } = Color.FromArgb(0x80, 0x82, 0x82, 0x82);
        public Color ButtonPressedForeground { get; set; } = Colors.Black;
        public Color ButtonPressedBackground { get; set; } = Color.FromArgb(0xB2, 0x82, 0x82, 0x82);
        public ICommand Open => new DelegateCommand(() => { });
    }
}
