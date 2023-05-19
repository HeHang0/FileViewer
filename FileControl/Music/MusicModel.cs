using FileViewer.FileHelper;
using NAudio.Wave;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FileViewer.FileControl.Music
{
    public class MusicModel : IFileModel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private IWavePlayer wavePlayer;
        private WaveStream reader;
        private DispatcherTimer timer = new DispatcherTimer();
        private string filePath;
        private string lastPlayed;
        private BitmapSource defaultThumbnail;

        public MusicModel()
        {
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += TimerOnTick;
            defaultThumbnail = Utils.GetBitmapSource(Properties.Resources.cloundmusic.ToBitmap());
        }

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            filePath = file.FilePath;
            StopCommand.Execute(null);
            GlobalNotify.OnLoadingChange(false);
            GlobalNotify.OnSizeChange(450, 500);
            SetThumbnailImage();
            OnColorChanged(Colors.White);
        }

        public void OnColorChanged(System.Windows.Media.Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        private void SetThumbnailImage()
        {
            try
            {
                //TagLib.File file = TagLib.File.Create(filePath);
                //if (file.Tag.Pictures.Count() > 0)
                //{
                //    using(MemoryStream ms = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                //    {
                //        ThumbnailImage = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                //        return;
                //    }
                //}
            }
            catch (Exception)
            {
            }
            ThumbnailImage = defaultThumbnail;
        }

        public ICommand OnUnLoaded => new DelegateCommand(() =>
        {
            Dispose();
        });

        public TimeSpan CurrentTime
        {
            get
            {
                if (reader == null)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return reader.CurrentTime;
                }
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                if (reader == null)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return reader.TotalTime;
                }
            }
        }

        public string ErrorMsg { get; private set; }
        public ImageSource ThumbnailImage { get; private set; }
        public ImageSource BackgroundImg => Utils.GetBitmapSource(Properties.Resources.disc);

        private double musicPosition;
        public double MusicPosition
        {
            get { return musicPosition; }
            set
            {
                if (musicPosition != value)
                {
                    musicPosition = value;
                    if (reader != null)
                    {
                        var pos = (long)(reader.Length * musicPosition / sliderMax);
                        reader.Position = pos; // media foundation will worry about block align for us
                    }
                    OnPropertyChanged("MusicPosition");
                }
            }
        }
        public float Vol
        {
            get
            {
                if (wavePlayer == null)
                {
                    return 0;
                }
                else
                {
                    return wavePlayer.Volume * 10;
                }
            }
            set
            {
                if(wavePlayer != null)
                {
                    wavePlayer.Volume = value / 10;
                    OnPropertyChanged("Vol");
                }
            }
        }

        const double sliderMax = 10.0;
        private void TimerOnTick(object sender, EventArgs e)
        {
            if (reader != null)
            {
                musicPosition = Math.Min(sliderMax, reader.Position * sliderMax / reader.Length);
                OnPropertyChanged("MusicPosition");
                OnPropertyChanged("CurrentTime");
            }
        }

        public ICommand PlayCommand => new DelegateCommand(() => {
            if (string.IsNullOrEmpty(filePath))
            {
                ErrorMsg = "Select a valid input file or URL first";
                return;
            }
            if (wavePlayer == null)
            {
                CreatePlayer();
            }
            if (lastPlayed != filePath && reader != null)
            {
                reader.Dispose();
                reader = null;
            }
            if (reader == null)
            {
                reader = new MediaFoundationReader(filePath);
                wavePlayer.Init(reader);
                OnPropertyChanged("TotalTime");
            }
            lastPlayed = filePath;
            Vol = 10f;
            wavePlayer.Play();
            OnPropertyChanged("IsPause");
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("IsStopped");
            OnPropertyChanged("Vol");
            timer.Start();
        });

        public ICommand PauseCommand => new DelegateCommand(() => {
            if (wavePlayer != null)
            {
                wavePlayer.Pause();
                OnPropertyChanged("IsPause");
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged("IsStopped");
            }
        });

        public ICommand StopCommand => new DelegateCommand(() => {
            if (wavePlayer != null)
            {
                wavePlayer.Stop();
                OnPropertyChanged("IsPause");
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged("IsStopped");
            }
        });

        public bool IsPause => wavePlayer == null ||
            wavePlayer.PlaybackState == PlaybackState.Stopped ||
             wavePlayer.PlaybackState == PlaybackState.Paused;

        public bool IsPlaying => wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing;

        public bool IsStopped=> wavePlayer == null || wavePlayer.PlaybackState == PlaybackState.Stopped;

        private void CreatePlayer()
        {
            wavePlayer = new WaveOutEvent();
            wavePlayer.PlaybackStopped += WavePlayerOnPlaybackStopped;
        }

        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (reader != null)
            {
                MusicPosition = 0;
                timer.Stop();
            }
            if (stoppedEventArgs.Exception != null)
            {
                ErrorMsg = stoppedEventArgs.Exception.Message+ "\nError Playing File";
            }
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("CurrentTime");
            OnPropertyChanged("IsStopped");
        }

        public void Dispose()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Dispose();
            }
            if (reader != null)
            {
                reader.Dispose();
            }
        }
    }
}
