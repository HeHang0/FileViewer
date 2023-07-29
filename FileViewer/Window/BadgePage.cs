using System.Windows.Controls;

namespace FileViewer
{
    public class BadgePage : Page
    {
        public delegate void BadgeShowEventHandler(object? sender, bool isBadgeShow);
        public event BadgeShowEventHandler? BadgeShowChanged;
        private bool _isBadgeShow = false;
        public bool IsBadgeShow => _isBadgeShow;
        protected void SetBadgeShow(bool isBadgeShow)
        {
            if (isBadgeShow == _isBadgeShow) return;
            _isBadgeShow = isBadgeShow;
            BadgeShowChanged?.Invoke(this, isBadgeShow);
        }
    }
}
