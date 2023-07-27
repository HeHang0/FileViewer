using FileViewer.Base;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace FileViewer.Plugins.Text
{
    /// <summary>
    /// Interaction logic for TextControl.xaml
    /// </summary>
    public partial class TextControl : UserControl
    {
        readonly IManager _manager;
        public TextControl(IManager manager)
        {
            _manager = manager;
            InitializeComponent();
            InitializeAvalon();
        }

        private readonly double height = SystemParameters.WorkArea.Height / 2;
        private readonly double width = SystemParameters.WorkArea.Width / 2;

        public void ChangeFile(string filePath)
        {
            ChangeTheme(_manager.IsDarkMode());
            var extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".ts" || extension == ".vue")
            {
                TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".js");
            }
            if (extension.StartsWith(".git"))
            {
                TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".ini");
            }
            else
            {
                TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(extension);
            }

            if (SetText(filePath))
            {
                if (TextEditor.SyntaxHighlighting == null && TextEditor.Document.Text.StartsWith("<"))
                {
                    TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
                }
                _manager.SetSize(height, width);
                _manager.SetResizeMode(true);
                _manager.SetLoading(false);
            }
        }

        private bool SetText(string filePath)
        {
            try
            {
                using StreamReader st = new(filePath, true);
                StringBuilder sb = new();
                string? str;
                int i = 0;
                for (; i < 1000; i++)
                {
                    str = st.ReadLine();
                    if (str == null) break;
                    sb.AppendLine(str);
                    if (sb.Length > 10000) break;
                }
                string result = sb.ToString()[..Math.Min(10000, sb.Length)];
                if (i == 1000 || sb.Length > 10000)
                {
                    result += "\r\n...";
                }
                TextEditor.Document.Text = result;
                return true;
            }
            catch (Exception)
            {
                _manager.LoadFileFailed(filePath);
            }
            return false;
        }

        public void ChangeTheme(bool dark)
        {
            var color = dark ? Color.FromRgb(0x33, 0x33, 0x33) : Color.FromRgb(0xEE, 0xF5, 0xFD);
            _manager?.SetColor(color);
            TextEditor.Background = new SolidColorBrush(color);
        }

        private static bool AvalonInitialized = false;
        private static void InitializeAvalon()
        {
            if (AvalonInitialized) return;
            AvalonInitialized = true;
            using MemoryStream stream = new(Properties.Resources.GolangSyntaxHighlighting);
            using XmlReader reader = new XmlTextReader(stream);
            var highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            HighlightingManager.Instance.RegisterHighlighting("Golang", new string[] { ".go" }, highlightingDefinition);
        }
    }
}