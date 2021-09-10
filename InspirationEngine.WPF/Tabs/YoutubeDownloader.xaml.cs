using InspirationEngine.Core;
using InspirationEngine.WPF.Models;
using static InspirationEngine.WPF.Utilities.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using YoutubeExplode.Playlists;
using System.Threading;
using YoutubeExplode.Videos;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Collections.Specialized;

namespace InspirationEngine.WPF.Tabs
{
    /// <summary>
    /// Interaction logic for YoutubeDownloader.xaml
    /// </summary>
    public partial class YoutubeDownloader : UserControl
    {
        /// <summary>
        /// A list of YouTube videos represented in the Video List DataGrid
        /// </summary>
        public ObservableCollection<YoutubeVideoModel> YoutubeVideos { get; set; } =
            new ObservableCollection<YoutubeVideoModel>();

        /// <summary>
        /// The SelectedItem on the Video List DataGrid
        /// </summary>
        public YoutubeVideoModel SelectedVideo
        {
            get { return (YoutubeVideoModel)GetValue(SelectedVideoProperty); }
            set { SetValue(SelectedVideoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedVideo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedVideoProperty =
            DependencyProperty.Register("SelectedVideo", typeof(YoutubeVideoModel), typeof(YoutubeDownloader), new PropertyMetadata(null, 
                (s, e) =>
                {
                    if (e.NewValue is YoutubeVideoModel video)
                    {
                        video.CurrentThumbnailIndex = 0;
                    }
                },
                (s, data) =>
                {
                    if (ReferenceEquals(data, CollectionView.NewItemPlaceholder))
                        data = null;
                    return data;
                }));

        /// <summary>
        /// The value bound to the Video List ProgressBar
        /// </summary>
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Progress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(YoutubeDownloader), new PropertyMetadata(0d));

        /// <summary>
        /// The Maximum value bound to the Video List ProgressBar
        /// </summary>
        public double MaxProgress
        {
            get { return (double)GetValue(MaxProgressProperty); }
            set { SetValue(MaxProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProgressProperty =
            DependencyProperty.Register("MaxProgress", typeof(double), typeof(YoutubeDownloader), new PropertyMetadata(1d));

        /// <summary>
        /// The text to display on the Video List ProgressBar
        /// </summary>
        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set { SetValue(ProgressTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(YoutubeDownloader), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The directory to save downloaded videos to
        /// </summary>
        public string ExportPath
        {
            get { return (string)GetValue(ExportPathProperty); }
            set { SetValue(ExportPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExportPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExportPathProperty =
            DependencyProperty.Register("ExportPath", typeof(string), typeof(YoutubeDownloader),
                new PropertyMetadata(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
                value => TryGetFullPath(value as string, out _));

        /// <summary>
        /// Bound to the Export File Destination radio group
        /// </summary>
        /// <remarks>
        /// [0] = Directly in Export Path
        /// [1] = Timestamped Subdirectory
        /// [2] = Timestamped Zip file
        /// </remarks>
        public bool[] ExportDestination
        {
            get { return (bool[])GetValue(ExportDestinationProperty); }
            set { SetValue(ExportDestinationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExportDestination.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExportDestinationProperty =
            DependencyProperty.Register("ExportDestination", typeof(bool[]), typeof(YoutubeDownloader), new PropertyMetadata(new[] { false, true, false }));

        /// <summary>
        /// Whether or not a cancellable operation is in progress
        /// </summary>
        public bool IsCancellable
        {
            get { return (bool)GetValue(IsCancellableProperty); }
            set { SetValue(IsCancellableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCancellable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCancellableProperty =
            DependencyProperty.Register("IsCancellable", typeof(bool), typeof(YoutubeDownloader), new PropertyMetadata(false));

        /// <summary>
        /// Whether or not the application is in a state where an Export is allowed
        /// </summary>
        public bool CanExport
        {
            get { return (bool)GetValue(CanExportProperty); }
            set { SetValue(CanExportProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanExport.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanExportProperty =
            DependencyProperty.Register("CanExport", typeof(bool), typeof(YoutubeDownloader), new PropertyMetadata(false));

        /// <summary>
        /// Whether an Export is currently in progress
        /// </summary>
        public bool IsDownloading
        {
            get { return (bool)GetValue(IsDownloadingProperty); }
            set { SetValue(IsDownloadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDownloading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDownloadingProperty =
            DependencyProperty.Register("IsDownloading", typeof(bool), typeof(YoutubeDownloader), new PropertyMetadata(false));

        /// <summary>
        /// Whether the user is in the middle of a drag and drop operation
        /// </summary>
        public bool IsDragDropDownloading
        {
            get { return (bool)GetValue(IsDragDropDownloadingProperty); }
            set { SetValue(IsDragDropDownloadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDragDropDownloading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragDropDownloadingProperty =
            DependencyProperty.Register("IsDragDropDownloading", typeof(bool), typeof(YoutubeDownloader), new PropertyMetadata(false));

        public YoutubeVideoDataObject DragDropDataObject { get; set; }


        /// <summary>
        /// The CancellationTokenSource to request tokens from
        /// </summary>
        /// <remarks>
        /// Pass into <see cref="RequestToken(ref CancellationTokenSource)"/> to get a new token
        /// </remarks>
        private CancellationTokenSource CancellationToken;

        /// <summary>
        /// Constructor
        /// </summary>
        public YoutubeDownloader()
        {
            InitializeComponent();
            DataContext = this;
            ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            YoutubeVideos.CollectionChanged += YoutubeVideos_CollectionChanged;
        }

        /// <summary>
        /// Invoked when the <see cref="YoutubeVideos"/> collection changes, or an item within the collection updates asynchronously as a result of a queried video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YoutubeVideos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<YoutubeVideoModel> videos)
            {
                CanExport =
                    videos.Count > 0 &&
                    TryGetFullPath(ExportPath, out _) &&
                    videos.Any(video => video.IsValid);
            }
        }

        /// <summary>
        /// Invoked when an editable NewItemPlaceholder field is double-clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridVideos_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            // enable InvokeSearch on new item, will search Title/URL once finished editing
            var newVideo = new YoutubeVideoModel() { InvokeSearch = true };
            newVideo.PropertyChanged += NewVideo_PropertyChanged;
            e.NewItem = newVideo;
        }

        /// <summary>
        /// Invoked when a video's properties have changed
        /// </summary>
        /// <remarks>
        /// Attach this event to each <see cref="YoutubeVideoModel.PropertyChanged"/> for each new video model created
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewVideo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Video reference can be updated asynchronously after the video has been added to the collection
            // Notify collection if the video has changed to allow for valid video searching for CanExport
            if (sender is YoutubeVideoModel video && video.IsValid && e.PropertyName == nameof(YoutubeVideoModel.Video))
            {
                YoutubeVideos_CollectionChanged(YoutubeVideos, null);
            }
        }

        /// <summary>
        /// Invoked right when a change is about to be committed after editing a DataGrid cell in the Video List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DataGridVideos_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // if edited column is a DataGridTemplateColumn (Title or URL)
            if (e.Column.Header is string header && 
                e.EditingElement is ContentPresenter templateColumnEditContent &&
                templateColumnEditContent.FindVisualChild<TextBox>() is not null and TextBox textBox)
            {
                switch(header)
                {
                    case "URL":
                        Progress = 0d;

                        // check if URL in editable cell is a Playlist URL
                        var playlistID = PlaylistId.TryParse(textBox.Text);
                        if (playlistID.HasValue)
                        {
                            var token = RequestToken(ref CancellationToken);
                            IsCancellable = true;

                            ProgressText = "Calculating Playlist Size...";
                            var youtube = new YoutubeInterface();

                            try
                            {
                                // retrieve total length of playlist
                                MaxProgress = await youtube.GetVideosInPlaylist(playlistID.Value, processBatch: true, token).CountAsync(token);

                                if (!token.IsCancellationRequested)
                                {
                                    try
                                    {
                                        // for each video in playlist
                                        await foreach (var (video, index) in youtube.GetVideosInPlaylist(playlistID.Value, processBatch: false, token).Select((v, i) => (v, i)))
                                        {
                                            if (token.IsCancellationRequested)
                                            {
                                                break;
                                            }

                                            ProgressText = $"Loading Video {index}/{MaxProgress}...";

                                            // First video in playlist should modify new item (item created from DataGridVideos_AddingNewItem)
                                            if (index == 0 && e.Row?.Item is YoutubeVideoModel newVideo)
                                            {
                                                try
                                                {
                                                    newVideo.Video = video;
                                                }
                                                catch (Exception)
                                                {
                                                    // Exception in YoutubeVideoModel::Video setter
                                                    newVideo.InvokeSearch = false;
                                                    newVideo.Title = "Unable to set Video object.";
                                                    newVideo.InvokeSearch = true;
                                                }
                                            }
                                            else
                                            {
                                                // Add next video in playlist to total videos collection
                                                // wait for dispatcher to finish doing it's thing to offer visual feedback on playlist updating
                                                await Dispatcher.InvokeAsync(() =>
                                                {
                                                    try
                                                    {
                                                        var newVideo = new YoutubeVideoModel()
                                                        {
                                                            Video = video,
                                                            InvokeSearch = true
                                                        };
                                                        newVideo.PropertyChanged += NewVideo_PropertyChanged;
                                                        YoutubeVideos.Add(newVideo);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        var errorVideo = new YoutubeVideoModel() { Title = "Unable to add Video to Collection" };
                                                        errorVideo.InvokeSearch = true;
                                                        errorVideo.PropertyChanged += NewVideo_PropertyChanged;
                                                        YoutubeVideos.Add(errorVideo);
                                                    }
                                                }, DispatcherPriority.ContextIdle);
                                            }

                                            // Increment progress bar
                                            Progress += 1d;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // exception occured in async iterator
                                        await Dispatcher.InvokeAsync(() =>
                                        {
                                            // set title before InvokeSearch is active
                                            var errorVideo = new YoutubeVideoModel() { Title = "Unable to query next video in Playlist" };
                                            errorVideo.InvokeSearch = true;
                                            errorVideo.PropertyChanged += NewVideo_PropertyChanged;
                                            YoutubeVideos.Add(errorVideo);
                                        }, DispatcherPriority.ContextIdle);
                                    }

                                    // do not commit change for current URL since change has already been made from modifying the first video
                                    e.Cancel = true;

                                    if (token.IsCancellationRequested)
                                    {
                                        ProgressText = $"Finished Loading {Progress}/{MaxProgress} Videos!";
                                        Progress = MaxProgress;
                                    }
                                    else
                                    {
                                        ProgressText = "Finished Loading Playlist!";
                                    }
                                }
                                else
                                {
                                    ProgressText = "Playlist search canceled.";
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                ProgressText = "Playlist search canceled.";
                            }
                            catch (Exception)
                            {
                                ProgressText = "Unable to calculate Playlist size.";
                            }

                            // Titles have been updated; autofit titles
                            if (sender is DataGrid dg)
                                dg.AutoFitColumn(dg.Columns.First(col => (string)col.Header == "Title"));

                            IsCancellable = false;
                            
                        }
                        // if not Playlist, let YoutubeVideoModel.PropertyChanged take care of loading the video
                        else
                        {
                            MaxProgress = 1d;
                        }
                        goto case "Title";
                    case "Title":
                        if (string.IsNullOrWhiteSpace(textBox.Text) && e.Row?.Item is YoutubeVideoModel emptyVideo)
                        {
                            // add something to the textbox to cause InvokeSearch to RemoveSelf once row has been committed
                            emptyVideo.RemoveSelf = YoutubeVideos;
                            textBox.Text = " ";
                        }
                        break;
                }
            }

            // Autofit edited column
            if (sender is DataGrid dataGrid)
            {
                dataGrid.AutoFitColumn(e.Column);
            }
        }

        /// <summary>
        /// Invoked when this control has been loaded
        /// </summary>
        /// <remarks>
        /// Since event handlers can be asynchronous, use this as an "async constructor"
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await FFmpegInterface.SetExecutablesPath(Properties.Settings.Default.ffmpegDir);
        }

        /// <summary>
        /// Invoked when the "Preview Video" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            SelectedVideo?.Navigate();
        }

        /// <summary>
        /// Invoked when a key is released on an attached TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Commit(object sender, KeyEventArgs e)
        {
            // Cause textbox to lose focus, invoking property update
            if (e.Key == Key.Return)
                (sender as FrameworkElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next) { Wrapped = true });
        }

        /// <summary>
        /// Invoked when "Copy image" is clicked from the Thumbnail Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                // Get Context Menu root within popup
                var contextMenu = item.FindParent<ContextMenu>();

                // contextMenu.PlacementTarget stores a reference to the object that invokes the ContextMenu
                if (contextMenu is not null && contextMenu.PlacementTarget is Image image && image.Source is BitmapImage bitmap)
                {
                    // Copy the image into the clipboard
                    Clipboard.SetImage(bitmap);
                }
            }
        }

        /// <summary>
        /// Invoked when "Save image as..." is clicked from the Thumbnail Context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveImageAs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                // Get Context Menu root within popup
                var contextMenu = item.FindParent<ContextMenu>();

                // contextMenu.PlacementTarget stores a reference to the object that invokes the ContextMeny
                if (contextMenu is not null && contextMenu.PlacementTarget is Image image && image.Source is BitmapImage bitmap)
                {
                    // Create new Save File Dialog with filters specifically for images
                    SaveFileDialog dlg = new SaveFileDialog()
                    {
                        Title = "Save Image As",
                        Filter = "Bitmap (*.bmp)|*.bmp|GIF (*.gif)|*.gif|JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|TIFF (*.tif)|*.tif|Windows Media Photo (*.wmp)|*.wmp|Supported Image Files|*.bmp;*.gif;*.jpeg;*.png;*.tif;*.wmp|All Files (*.*)|*.*",
                        FilterIndex = 7,
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        DefaultExt = "png"
                    };

                    // If user chooses to save
                    if (dlg.ShowDialog() == true)
                    {
                        // define generic encoder
                        BitmapEncoder encoder;
                        var ext = Path.GetExtension(dlg.FileName).ToLower().TrimStart('.');

                        // instantiate encoder as specific object via file extension
                        switch(ext)
                        {
                            case "bmp":
                                encoder = new BmpBitmapEncoder();
                                break;
                            case "gif":
                                encoder = new GifBitmapEncoder();
                                break;
                            case "jpeg":
                                encoder = new JpegBitmapEncoder();
                                break;
                            case "png":
                                encoder = new PngBitmapEncoder();
                                break;
                            case "tif":
                                encoder = new TiffBitmapEncoder();
                                break;
                            case "wmp":
                                encoder = new WmpBitmapEncoder();
                                break;
                            default:
                                // png is the default because png is c o o l
                                goto case "png";
                        }

                        // polymorphism rules!!1
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));

                        using (var fileStream = new FileStream(dlg.FileName, FileMode.Create))
                        {
                            // Save image using specified encoder
                            encoder.Save(fileStream);
                        }
                    }
                }
            }    
        }

        /// <summary>
        /// Invoked when the little X by the Video List ProgressBar is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancellationToken?.Cancel();
        }

        /// <summary>
        /// Invoked when the "Export" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            // If not within a current download and if exportable and if ffmpeg is valid
            if (!IsDownloading && CanExport && (await FFmpegInterface.Verify() == null))
            {
                // Disable button during export process
                CanExport = false;
                IsDownloading = true;

                // set export directory to optional timestamped subdirectory
                string timestamp = DateTime.Now.ToString("yyyyMMdd-hhmmss");
                string exportDir = ExportDestination[0] ? ExportPath :
                    Path.Combine(ExportPath, timestamp);
                if (!ExportDestination[0])
                    Directory.CreateDirectory(exportDir);

                var token = RequestToken(ref CancellationToken);
                IsCancellable = true;

                // filter invalid videos out
                var validVideos = YoutubeVideos.Where(v => v.IsValid);
                MaxProgress = validVideos.Count();

                Progress = 0d;
                void UpdateProgressBar() => ProgressText = $"Downloading Videos... ({Progress}/{MaxProgress})";

                UpdateProgressBar();
                try
                {
                    // download all videos in parallel
                    await Task.WhenAll(
                        validVideos
                            // project videos list as a list of Download/Trim tasks
                            .Select(async v =>
                            {
                                // if redownloading after a failed attempt, make sure video title is correct
                                const string FailureMessage = "Failed to download \"";
                                bool isInvoke = v.InvokeSearch;
                                v.InvokeSearch = false;
                                if (v.Title.StartsWith(FailureMessage))
                                {
                                    // get substring starting from the end of the failure message to right before the trailing quote mark
                                    v.Title = v.Title[FailureMessage.Length..^1];
                                }

                                // download video to exportDir
                                if (await v.Download(exportDir, FFmpegInterface.ExecutablePath.ffmpeg, token) == null)
                                {
                                    // if video failed to download, update gui accordingly
                                    v.Title = $"Failed to download \"{v.Title}\"";
                                }
                                v.InvokeSearch = isInvoke;

                                // Increment main progress bar
                                Progress += 1d;
                                UpdateProgressBar();
                            })
                        );

                    ProgressText = $"Files successfully downloaded to {exportDir}";
                }
                catch (TaskCanceledException)
                {
                    ProgressText = $"Export has been canceled.";
                }

                // Re-enable button once finished
                IsCancellable = false;
                IsDownloading = false;
                CanExport = true;
            }
        }

        private async void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Image img && 
                (SelectedVideo?.IsValid ?? false) && 
                e.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsDragDropDownloading)
                {
                    IsDragDropDownloading = true;
                    var currentCursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = Cursors.Wait;
                    var video = await YoutubeVideoDataObject.Construct(SelectedVideo);
                    Mouse.OverrideCursor = currentCursor;
                    DragDrop.DoDragDrop(img, video.data, DragDropEffects.Move);
                    IsDragDropDownloading = false;
                }
            }
        }
    }
}
