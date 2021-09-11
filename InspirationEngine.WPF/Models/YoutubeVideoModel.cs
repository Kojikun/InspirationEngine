using InspirationEngine.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YoutubeExplode.Videos;
using YoutubeExplode.Exceptions;
using System.Diagnostics;
using System.IO;
using YoutubeExplode.Common;
using System.Threading;
using System.Text.RegularExpressions;
using InspirationEngine.WPF.Utilities;

namespace InspirationEngine.WPF.Models
{
    /// <summary>
    /// Data model containing bindable YouTube video metadata
    /// </summary>
    public class YoutubeVideoModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        /// <summary>
        /// Default Constructor: InvokeSearch = false; ExportFormat = "wav"
        /// </summary>
        public YoutubeVideoModel()
        {
            ExportFormat = "wav";
            PropertyChanged += YoutubeVideoModel_PropertyChanged;
            DownloadProgress.ProgressChanged += (s, val) => DownloadProgressValue = val;
        }

        /// <summary>
        /// INotifyPropertyChanged event handler
        /// Invokes special behavior if InvokeSearch is true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void YoutubeVideoModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (InvokeSearch)
            {
                if (RemoveSelf != null)
                {
                    RemoveSelf.Remove(this);
                }
                else
                {
                    switch (e.PropertyName)
                    {
                        case nameof(Video):
                            if (Video is not null)
                            {
                                StreamUrl = new Uri(await new YoutubeInterface().GetVideoStream(Url));
                            }
                            break;
                        case nameof(StreamUrl):
                            if (StreamUrl is not null)
                            {
                                PreviewViewable = await StreamUrl.IsAccessibleAsync();
                            }
                            break;
                        case nameof(Title):
                            // Get First Video using new title value
                            try
                            {
                                Video = await new YoutubeInterface().SearchVideo(Title).FirstOrDefaultAsync();
                                if (Video is null)
                                {
                                    InvokeSearch = false;
                                    Title = "No results found";
                                    InvokeSearch = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                InvokeSearch = false;
                                Title = ex.Message;
                                IsValid = false;
                                InvokeSearch = true;
                            }
                            break;
                        case nameof(Url):
                            // Search video using new Url property value
                            try
                            {
                                // attempt to query url
                                Video = await new YoutubeInterface().GetYoutubeVideo(Url);
                            }
                            catch (Exception ex)
                            {
                                InvokeSearch = false;
                                Title = ex.Message;
                                IsValid = false;
                                InvokeSearch = true;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Whether or not to invoke any asynchronous operations upon PropertyChanged
        /// </summary>
        public bool InvokeSearch { get; set; }

        /// <summary>
        /// Container of all the video metadata
        /// </summary>
        private IVideo _Video;
        public IVideo Video
        {
            get => _Video;
            set
            {
                _Video = value;

                // never invoke search upon changing video reference
                var invokeSearchTemp = InvokeSearch;
                InvokeSearch = false;

                IsValid = Video is not null;
                Title = Video?.Title;
                Url = Video?.Url;
                NotifyPropertyChanged(nameof(Duration));
                TimeStart = new TimeSpan();
                TimeEnd = Duration ?? new TimeSpan();
                Thumbnails.Clear();
                foreach(var thumb in Video?.Thumbnails ?? Enumerable.Empty<Thumbnail>())
                {
                    Thumbnails.Add(new BitmapImage(new Uri(thumb.Url))
                    {
                        DecodePixelHeight = thumb.Resolution.Height,
                        DecodePixelWidth = thumb.Resolution.Width
                    });
                }
                CurrentThumbnailIndex = Video is null ? -1 : 0;

                // restore invoke search value
                InvokeSearch = invokeSearchTemp;

                // invoke async changes
                NotifyPropertyChanged();
            }
        }


        private bool _IsValid;
        /// <summary>
        /// Whether this object contains all the necessary data for a well-formed video
        /// </summary>
        public bool IsValid
        {
            get => _IsValid;
            set
            {
                _IsValid = value;
                NotifyPropertyChanged();
            }
        }


        private string _Title;
        /// <summary>
        /// The YouTube Video Title
        /// </summary>
        public string Title
        {
            get => _Title;
            set
            {
                if (value != Title)
                {
                    _Title = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private string _Url;
        /// <summary>
        /// The website link to the YouTube Video
        /// </summary>
        public string Url
        {
            get => _Url;
            set
            {
                if (value != Url)
                {
                    _Url = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The total length of the video from start to finish
        /// </summary>
        public TimeSpan? Duration { get => Video?.Duration; }


        private TimeSpan _TimeStart;
        /// <summary>
        /// The time in the video to begin the exported audio at
        /// </summary>
        public TimeSpan TimeStart
        {
            get => _TimeStart;
            set
            {
                _TimeStart = value < TimeEnd ? value : TimeEnd;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Length");
            }
        }

        private TimeSpan _TimeEnd;
        /// <summary>
        /// The time in the video to end the exported audio at
        /// </summary>
        public TimeSpan TimeEnd
        {
            get => _TimeEnd;
            set
            {
                _TimeEnd = 
                    // lower-bound value by TimeStart
                    value <= TimeStart ? TimeStart : 
                    // if TotalSeconds == Duration, use duration value
                    (Math.Floor(value.TotalSeconds) == Math.Floor(Duration?.TotalSeconds ?? 0) ? Duration.Value : value);
                NotifyPropertyChanged();
                NotifyPropertyChanged("Length");
            }
        }

        /// <summary>
        /// The length of the exported audio
        /// </summary>
        public TimeSpan Length { get => TimeEnd - TimeStart; }

        /// <summary>
        /// A list of Thumbnail images received from querying the video
        /// </summary>
        public ObservableCollection<BitmapImage> Thumbnails =
            new ObservableCollection<BitmapImage>();


        private int _CurrentThumbnailIndex;
        /// <summary>
        /// The index of the displayed thumbnail within the list of Thumbnails
        /// </summary>
        public int CurrentThumbnailIndex
        {
            get => _CurrentThumbnailIndex;
            set
            {
                _CurrentThumbnailIndex = value < -1 ? -1 : (value > Thumbnails.Count ? Thumbnails.Count - 1 : value);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CurrentThumbnail));
            }
        }

        /// <summary>
        /// The thumbnail currently being displayed to the user
        /// </summary>
        public BitmapImage CurrentThumbnail { get => Thumbnails.ElementAtOrDefault(CurrentThumbnailIndex); }


        private Uri _StreamUrl;
        public Uri StreamUrl
        {
            get => _StreamUrl;
            set
            {
                _StreamUrl = value;
                NotifyPropertyChanged();
            }
        }

        private bool _PreviewViewable;
        public bool PreviewViewable
        {
            get => _PreviewViewable;
            set
            {
                _PreviewViewable = value;
                NotifyPropertyChanged();
            }
        }


        private string _ExportFormat;
        /// <summary>
        /// The exported audio file extension for the Video
        /// </summary>
        public string ExportFormat
        {
            get => _ExportFormat;
            set
            {
                // ensure that the format has no leading (or trailing lol) ., no whitespace and is lowercase
                _ExportFormat = Regex.Replace(value, @"\s+", "", RegexOptions.Singleline).Trim('.').ToLowerInvariant();
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// A reference to the parent collection that, when not null, will remove itself from
        /// </summary>
        /// <remarks>
        /// This is a super hacky way of implementing self-removal when a row has been committed after DataGrid_CellEditEnding
        /// Removing the new row from that event will remove the NewRowPlaceholder, which prevents any extra videos from being added
        /// This property should almost never not be null except in this niche use case
        /// Removal happens in <see cref="PropertyChanged"/> since, prior to this reference being updated, empty space is added
        /// to the textbox to invoke the PropertyChanged hander once the cell has been committed to the grid
        /// 
        /// Yea this sucks, but what are you gonna do about it lmao
        /// </remarks>
        public ObservableCollection<YoutubeVideoModel> RemoveSelf { get; set; }

        /// <summary>
        /// Opens a web browser displaying the current video starting at TimeStart
        /// </summary>
        /// <returns>Returns true if web browser process has successfully executed</returns>
        public bool Navigate()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(
                    new Uri($"{Url}{(TimeStart.TotalSeconds > 0 ? $"&t={Math.Round(TimeStart.TotalSeconds)}s" : string.Empty)}").AbsoluteUri)
                {
                    UseShellExecute = true
                }
            };
            return process.Start();
        }

        /// <summary>
        /// Downloads the audio from the video to a given file path
        /// </summary>
        /// <param name="downloadDirectory">The directory to save the file to</param>
        /// <param name="ffmpegPath">The path of the ffmpeg executable to use for download conversion</param>
        /// <param name="cancellationToken">Token that can be used to cancel the async operation</param>
        /// <returns>Returns the fully qualified path of the downloaded file, or null if unsuccessful</returns>
        public async Task<string> Download(string downloadDirectory, string ffmpegPath, CancellationToken cancellationToken = default)
        {
            // trimming operation invoked if TimeStart/End are not default values
            bool needsTrim = TimeStart.TotalSeconds > 0 || TimeEnd != Duration;

            // download file as "temp" if it needs trimming
            // not sure how to directly download trimmed videos using YoutubeExplode, so unfortunately,
            // the full video will be downloaded first and then it'll be trimmed
            string filePath = Path.Combine(downloadDirectory, $"{Title.ToValidFileName()}{(needsTrim ? "_temp" : "")}.{ExportFormat}").EnsureUniqueFile();

            try
            {
                // invoke download
                await new YoutubeInterface().Download(Url, filePath, ExportFormat, ffmpegPath, DownloadProgress, cancellationToken);
            }
            catch (Exception)
            {
                // download failed lol
                return null;
            }

            // if trimming is required, "filePath" is the name of a temp file
            if (needsTrim)
            {
                // final file name will be without _temp suffix
                string filePath_temp = filePath;
                filePath = Path.Combine(downloadDirectory, $"{Title.ToValidFileName()}.{ExportFormat}").EnsureUniqueFile();

                // trim temp file and save as final file
                await FFmpegInterface.Trim(filePath_temp, filePath, TimeStart, TimeEnd != Duration ? TimeEnd : null, cancellationToken);

                // delete temp file
                File.Delete(filePath_temp);
            }

            return filePath;
        }

        /// <summary>
        /// A progress object that is updated during a download operation, where 0 is no progress and 1 is ALL progress
        /// </summary>
        public Progress<double> DownloadProgress { get; set; } = new Progress<double>();


        private double _DownloadProgressValue;
        /// <summary>
        /// The value returned from <see cref="DownloadProgress"/>
        /// </summary>
        public double DownloadProgressValue
        {
            get => _DownloadProgressValue;
            set
            {
                _DownloadProgressValue = value;
                NotifyPropertyChanged();
            }
        }
    }
}
