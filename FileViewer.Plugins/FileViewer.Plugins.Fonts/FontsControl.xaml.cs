using FileViewer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Fonts
{
    /// <summary>
    /// Interaction logic for FontsControl.xaml
    /// </summary>
    public partial class FontsControl : UserControl
    {
        readonly IManager? _manager;
        public FontsControl(IManager manager)
        {
            _manager = manager;
            InitializeComponent();
        }

        public void ChangeFile(string filePath)
        {
            _manager?.SetResizeMode(true);
            ChangeTheme(_manager!.IsDarkMode());
            GetAllFontCharAsync(filePath).ContinueWith((e) =>
            {
                _manager?.SetLoading(false);
            });
        }

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundLight);
        }

        public class FontItem
        {
            public FontFamily FontFamily { get; }
            public string FontFamilyName { get; }
            public int Value { get; }
            public string Text { get; }
            public string CharHexH { get; }
            public string CharHexU { get; }
            public string TypeName { get; }
            public FontItem(FontFamily family, string familyName, string typeName, int value)
            {
                FontFamily = family;
                TypeName = typeName;
                Value = value;
                FontFamilyName = familyName;
                Text = char.ConvertFromUtf32(value);
                var v = value.ToString("X").PadLeft(4, '0');
                CharHexH = $"&#x{v};";
                CharHexU = "\\u" + v;
            }
        }

        private async Task GetAllFontCharAsync(string filePath)
        {
            await Task.Run(() =>
            {
                var result = GetAllFontChar(filePath);
                Dispatcher.Invoke(() =>
                {
                    FontsView.ItemsSource = result;
                });
            });
        }

        private List<FontItem> GetAllFontChar(string filePath)
        {

            List<FontItem> result = new();
            try
            {
                var families = System.Windows.Media.Fonts.GetFontFamilies(filePath);
                foreach (FontFamily family in families)
                {
                    var fontFamily = new FontFamily(family.Source);
                    var familyName = family.FamilyNames.FirstOrDefault().Value ?? string.Empty;
                    var typefaces = family.GetTypefaces();
                    foreach (Typeface typeface in typefaces)
                    {
                        typeface.TryGetGlyphTypeface(out GlyphTypeface glyph);
                        IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;
                        var typeName = typeface.FaceNames.FirstOrDefault().Value ?? string.Empty;
                        foreach (KeyValuePair<int, ushort> kvp in characterMap)
                        {
                            result.Add(new FontItem(fontFamily, familyName, typeName, kvp.Key));
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}