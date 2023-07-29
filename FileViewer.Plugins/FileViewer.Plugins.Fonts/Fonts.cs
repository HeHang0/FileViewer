using FileViewer.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileViewer.Plugins.Fonts
{
    public class Fonts: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        readonly IManager? _manager;
        public Fonts(IManager manager)
        {
            _manager = manager;
        }

        public ObservableCollection<FontItem> FontItems => _currentList;

        public double FontsWidth { get; set; } = 0;

        public double FontsHeight { get; set; } = 0;

        public bool LastPageEnabled => _page > 0 && _page < _total;

        public bool NextPageEnabled => _page < _total - 1;

        public ICommand SizeChanged => new DelegateCommand<SizeChangedEventArgs>(ResetPageSize);

        public ICommand NextPage => new DelegateCommand(IncreaseIndex);

        public ICommand LastPage => new DelegateCommand(DecreaseIndex);

        List<FontItem> _allList = new();
        readonly ObservableCollection<FontItem> _currentList = new();
        int _page = 0;
        int _total = 0;
        int _pageSize = 0;
        readonly double defaultWidth = 771;
        readonly double defaultHeight = 458;
        double viewWidth = 730;
        double viewHeight = 420;

        readonly Tools.Timeout resetSizeTimeout = new();
        readonly Dispatcher CurrentDispatcher = Dispatcher.CurrentDispatcher;
        public void ResetPageSize(SizeChangedEventArgs? e = null)
        {
            if(e != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                viewWidth = e.NewSize.Width;
                viewHeight = e.NewSize.Height;
            }
            _manager?.SetLoading(true);
            resetSizeTimeout.SetTimeout(() =>
            {
                CurrentDispatcher.Invoke(FillUpList);
            }, TimeSpan.FromMilliseconds(100));
        }

        public void DecreaseIndex()
        {
            _page--;
            FillUpList();
        }

        public void IncreaseIndex()
        {
            _page++;
            FillUpList();
        }

        public void FillUpList()
        {
            _pageSize = ((int)viewWidth / 121) * ((int)viewHeight / 105);
            _currentList.Clear();
            _total = (int)Math.Ceiling((double)_allList.Count / _pageSize);
            var requestList = _allList.Skip(_page * _pageSize).Take(_pageSize);
            if(!requestList.Any())
            {
                requestList = _allList.TakeLast(_pageSize);
            }
            foreach (var item in requestList)
            {
                _currentList.Add(item);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontItems)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPageEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextPageEnabled)));
            _manager?.SetLoading(false);
        }

        public void ChangeFile(string filePath)
        {
            _manager?.SetResizeMode(true);
            _manager?.SetSize(defaultHeight, defaultWidth);
            ChangeTheme(_manager!.IsDarkMode());
            _page = 0;
            _allList = GetAllFontChar(filePath);
            ResetPageSize();
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

        private Task<List<FontItem>> GetAllFontCharAsync(string filePath)
        {
            return Task.Run(() =>
            {
                return GetAllFontChar(filePath);
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
