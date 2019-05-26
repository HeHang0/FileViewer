using FileViewer.FileHelper;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FileViewer.FileControl.Video
{
    public class VideoModel : INotifyPropertyChanged, IFileChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string filePath;
        private DispatcherTimer timer = new DispatcherTimer();
        private System.Windows.Controls.MediaElement mediaPlayer;
        public VideoModel()
        {
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += TimerOnTick;
        }



        private void TimerOnTick(object sender, EventArgs e)
        {
            if(mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                videoPosition = 10 * mediaPlayer.Position.TotalMilliseconds / mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                OnPropertyChanged("VideoPosition");
                OnPropertyChanged("CurrentTime");
            }
        }

        public TimeSpan CurrentTime => mediaPlayer == null ? TimeSpan.Zero : mediaPlayer.Position;
        public bool IsPause { get; private set; }
        public bool IsPlaying { get; private set; }
        public double ControlsOpacity { get; private set; } = 1;

        public Duration TotalTime => mediaPlayer == null ? new Duration(TimeSpan.Zero) : mediaPlayer.NaturalDuration;

        private double videoPosition;
        private const int sliderMax = 10;
        public double VideoPosition
        {
            get { return videoPosition; }
            set
            {
                if (videoPosition != value)
                {
                    videoPosition = value;

                    if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        var pos = (mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds * videoPosition / sliderMax);
                        mediaPlayer.Position = TimeSpan.FromMilliseconds(pos);
                    }
                    OnPropertyChanged("VideoPosition");
                }
            }
        }
        public double Vol
        {
            get
            {
                if (mediaPlayer == null)
                {
                    return 0;
                }
                else
                {
                    return mediaPlayer.Volume * 10;
                }
            }
            set
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.Volume = value / 10;
                    OnPropertyChanged("Vol");
                }
            }
        }

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            filePath = file.FilePath;
            StopCommand.Execute(null);
            GlobalNotify.OnLoadingChange(false);
            GlobalNotify.OnSizeChange(730, 1280);
            if (mediaPlayer != null && mediaPlayer.Source?.AbsolutePath != new Uri(filePath).AbsolutePath)
            {
                mediaPlayer.Source = new Uri(filePath);
            }
            ControlsOpacity = 1;
        }

        public ICommand GridLoaded => new DelegateCommand<System.Windows.Controls.MediaElement>((player) => {
            mediaPlayer = player;
            if (mediaPlayer.Source?.AbsolutePath != new Uri(filePath).AbsolutePath)
            {
                mediaPlayer.Source = new Uri(filePath);
            }
            GlobalNotify.WindowClose += WindowClose;
        });

        private void WindowClose()
        {
            mediaPlayer?.Pause();
        }

        public ICommand GridUnloaded => new DelegateCommand(() => {
            mediaPlayer?.Close();
            GlobalNotify.WindowClose -= WindowClose;
        });

        public ICommand ControlsShow => new DelegateCommand(() => {
            ControlsOpacity = 1;
        });

        public ICommand ControlsHide => new DelegateCommand(() => {
            ControlsOpacity = 0;
        });

        public ICommand PlayCommand => new DelegateCommand(() => {
            mediaPlayer?.Play();
            timer.Start();
            IsPause = false;
            IsPlaying = true;
            OnPropertyChanged("Vol");
        });

        public ICommand PauseCommand => new DelegateCommand(() => {
            mediaPlayer?.Pause();
            timer.Stop();
            IsPause = true;
            IsPlaying = false;
        });

        public ICommand StopCommand => new DelegateCommand(() => {
            mediaPlayer?.Stop();
            timer.Stop();
            IsPause = true;
            IsPlaying = false;
        });

        private bool isFullScreen = false;
        public ICommand FullScreenCommand => new DelegateCommand(() => {
            isFullScreen = !isFullScreen;
            GlobalNotify.OnFullScreen(isFullScreen);
        });


    }
}
