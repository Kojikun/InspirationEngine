using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using InspirationEngine.Core;
using InspirationEngine.WPF.Utilities;

namespace InspirationEngine.WPF.Models
{
    /// <summary>
    /// Contains a DataObject that represents an exported audio file from a YouTube video
    /// </summary>
    public class YoutubeVideoDataObject
    {
        public DataObject data;

        public static implicit operator DataObject(YoutubeVideoDataObject d) => d.data;

        private YoutubeVideoDataObject()
        {
            data = new DataObject();
        }

        public static async Task<YoutubeVideoDataObject> Construct(YoutubeVideoModel video, CancellationToken cancellationToken = default)
        {
            var dir = Properties.Settings.Default.SamplesDir;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var videoFileName = await video.Download(dir, FFmpegInterface.ExecutablePath.ffmpeg, cancellationToken);

            var videoDataObject = new YoutubeVideoDataObject();
            var fileCollection = new StringCollection();
            fileCollection.Add(videoFileName);
            videoDataObject.data.SetFileDropList(fileCollection);

            return videoDataObject;
        }
    }
}
