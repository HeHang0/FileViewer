using FileViewer.Base;
using FileViewer.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FileViewer.Plugins.Media
{
    /// <summary>
    /// Interaction logic for MediaControl.xaml
    /// </summary>
    public partial class MediaControl : UserControl
    {
        readonly IManager _manager;
        string? _filePath;
        bool _dragging = false;
        bool _isMouseDown = false;
        readonly Timeout timeout = new();
        readonly Timeout progressTimeout = new();
        readonly DispatcherTimer timer = new();
        public MediaControl(IManager manager)
        {
            _manager = manager;
            InitializeComponent();
            MediaPlayer.MediaOpened += MediaOpened;
            MediaPlayer.MediaFailed += MediaFailed;
            MediaPlayer.MediaEnded += MediaEnded;
            MouseMove += MediaPlayer_MouseMove;
            MouseLeave += MediaPlayer_MouseLeave;
            MouseEnter += MediaPlayer_MouseMove;
            IsVisibleChanged += OnVisibleChanged;
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += TimerOnTick;
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible && _isPlaying)
            {
                SwitchPlayState(sender, null);
            }
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (!_dragging && !_isMouseDown) MediaProgress.Value = MediaPlayer.Position.TotalSeconds;
            MediaProgress1.Value = MediaPlayer.Position.TotalSeconds;
            CurrentTime.Content = MediaPlayer.Position.ToString("hh\\:mm\\:ss");
        }

        private void MediaProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void MediaProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private void MediaProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MediaPlayer.Position = TimeSpan.FromSeconds(MediaProgress.Value);
            _dragging = false;
            _isMouseDown = false;
        }

        private void MediaProgress_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MediaPlayer.Position = TimeSpan.FromSeconds(MediaProgress.Value);
            progressTimeout.ClearTimeout();
            _isMouseDown = false;
        }

        private void MediaProgress_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            ResetDraging();
        }

        private void ResetDraging()
        {
            progressTimeout.SetTimeout(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    _isMouseDown = false;
                });
            }, 200);
        }

        private void MediaPlayer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ControlPannel.Opacity = 0;
            timeout.ClearTimeout();
        }

        private void MediaPlayer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ControlPannel.Opacity != 1)
            {
                ControlPannel.Opacity = 1;
            }
            timeout.ClearTimeout();
            timeout.SetTimeout(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    ControlPannel.Opacity = 0;
                });
            }, 5000);
        }

        private void MediaFailed(object? sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            timer.Stop();
            if (!string.IsNullOrEmpty(_filePath)) _manager.LoadFileFailed(_filePath);
            _isPlaying = false;
            PlayButton.Content = "▶";
        }

        private bool _isPlaying = false;
        private void MediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            _manager.SetResizeMode(MediaPlayer.HasVideo);
            if (MediaPlayer.HasVideo)
            {
                _manager.SetSize(450, 800);
                _titleBarHeight = 450 - ActualHeight;
                ResetSize();
            }
            else
            {
                _manager.SetSize(500, 400);
            }
            MediaProgress.Maximum = MediaPlayer.NaturalDuration.HasTimeSpan ? MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds : 0;
            MediaProgress.Value = 0;
            MediaProgress1.Maximum = MediaPlayer.NaturalDuration.HasTimeSpan ? MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds : 0;
            MediaProgress1.Value = 0;
            CurrentTime.Content = new TimeSpan().ToString("hh\\:mm\\:ss");
            TotalTime.Content = MediaPlayer.NaturalDuration.HasTimeSpan ? MediaPlayer.NaturalDuration.TimeSpan.ToString("hh\\:mm\\:ss") : CurrentTime.Content;
            timer.Start();
            PlayButton.Content = "⏸️";
            _isPlaying = true;
            MusicLabel.Opacity = MediaPlayer.HasVideo ? 0 : 1;
            _manager.SetLoading(false);
        }

        private void SwitchPlayState(object sender, System.Windows.Input.MouseButtonEventArgs? e)
        {
            try
            {
                if (_isPlaying)
                {
                    if (MediaPlayer.CanPause) MediaPlayer.Pause();
                    PlayButton.Content = "▶️";
                    timer.Stop();
                }
                else
                {
                    MediaPlayer.Play();
                    PlayButton.Content = "⏸️";
                    timer.Start();
                }
            }
            catch (Exception)
            {
            }
            _isPlaying = !_isPlaying;
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            _isPlaying = false;
            PlayButton.Content = "▶️";
            MediaPlayer.Stop();
            MediaPlayer.Position = TimeSpan.Zero;
        }

        public MediaElement Player => MediaPlayer;

        private double _titleBarHeight = 0;

        public void ChangeFile(string filePath)
        {
            _filePath = filePath;
            MediaPlayer.Source = new Uri(filePath);
            ChangeTheme(_manager.IsDarkMode());
            try
            {
                MediaPlayer.Play();
            }
            catch (Exception)
            {
            }
            MediaPlayer.IsMuted = true;
            MuteButton.Content = "🔇";
        }

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundLight);
        }

        private void ResetSize()
        {
            double targetAspectRatio = (double)MediaPlayer.NaturalVideoWidth / MediaPlayer.NaturalVideoHeight;

            double targetWidthH = ActualHeight * targetAspectRatio;
            double targetHeightH = targetWidthH / targetAspectRatio;

            double targetHeightW = ActualWidth / targetAspectRatio;
            double targetWidthW = targetHeightW * targetAspectRatio;

            double areaH = targetWidthH * targetHeightH;
            double areaW = targetHeightW * targetWidthW;
            double areaA = ActualWidth * ActualHeight;

            targetHeightW += _titleBarHeight;
            targetHeightH += _titleBarHeight;
            if (Math.Abs(areaA - areaH) > Math.Abs(areaA - areaW))
            {
                _manager.SetSize(targetHeightW, targetWidthW);
            }
            else
            {
                _manager.SetSize(targetHeightH, targetWidthH);
            }
        }

        private void MuteButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
            MuteButton.Content = MediaPlayer.IsMuted ? "🔇" : "🔈";
        }
    }
}