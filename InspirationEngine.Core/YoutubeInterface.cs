using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace InspirationEngine.Core
{
    /// <summary>
    /// Defines an instance of a YouTube client
    /// </summary>
    public class YoutubeInterface
    {
        private YoutubeClient youtube;

        public YoutubeInterface()
        {
            youtube = new YoutubeClient();
        }

        /// <summary>
        /// Returns a list of PlaylistVideos for a given Playlist URL
        /// </summary>
        /// <param name="url">A string specifying a YouTube Playlist ID or URL</param>
        /// <param name="processBatch">Whether to retrieve videos as batches</param>
        /// <param name="cancellationToken">Token that can be used to cancel the async operation</param>
        /// <returns>Return a list of PlaylistVideos</returns>
        public async IAsyncEnumerable<PlaylistVideo> GetVideosInPlaylist(PlaylistId url, bool processBatch = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (processBatch)
            {
                // for each batch of videos queried
                await foreach (var batch in youtube.Playlists.GetVideoBatchesAsync(url, cancellationToken))
                {
                    // yield return videos in batch
                    foreach (var video in batch.Items)
                    {
                        yield return video;
                    }
                }
            }
            else
            {
                // yield return videos in playlist
                await foreach (var video in youtube.Playlists.GetVideosAsync(url, cancellationToken))
                {
                    yield return video;
                }
            }
        }

        /// <summary>
        /// Return a Video object for a given Video URL
        /// </summary>
        /// <param name="url">A string specifying a YouTube Video ID or URL</param>
        /// <param name="cancellationToken">Token that can be used to cancel the async operation</param>
        /// <returns>Return a video object</returns>
        public async ValueTask<Video> GetYoutubeVideo(VideoId url, CancellationToken cancellationToken = default) =>
            await youtube.Videos.GetAsync(url, cancellationToken);

        /// <summary>
        /// Returns a list of videos that satisfy the search query
        /// </summary>
        /// <param name="query">The search query to search videos for</param>
        /// <param name="processBatch">Whether to return videos in batches</param>
        /// <param name="cancellationToken">Token that can be used to cancel the async operation</param>
        /// <returns>Returns a list of Videos that are a result of the Search Result</returns>
        public async IAsyncEnumerable<VideoSearchResult> SearchVideo(string query, bool processBatch = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (processBatch)
            {
                // for each batch of vvideos queried
                await foreach(var batch in youtube.Search.GetResultBatchesAsync(query, cancellationToken))
                {
                    // yield return each video result
                    foreach(var result in batch.Items)
                    {
                        switch (result)
                        {
                            case VideoSearchResult videoResult:
                                yield return videoResult;
                                break;
                        }
                    }
                }
            }
            else
            {
                // yield return each video result
                await foreach(var result in youtube.Search.GetVideosAsync(query, cancellationToken))
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Download a Video's audio from a URL using ffmpeg
        /// </summary>
        /// <param name="url">A string specifying a YouTube Video ID or URL</param>
        /// <param name="downloadPath">A file path to save the resulting audio file to</param>
        /// <param name="format">The file extension of the saved file</param>
        /// <param name="ffmpegPath">The path to the ffmpeg executable</param>
        /// <param name="progress">Progress object to send download progress to</param>
        /// <param name="cancellationToken">Token that can be used to cancel the async operation</param>
        /// <returns>async void</returns>
        public async ValueTask Download(VideoId url, string downloadPath, string format, string ffmpegPath, IProgress<double> progress, CancellationToken cancellationToken = default)
        {
            // get video metadata
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url, cancellationToken);

            // get best audio info from metadata
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // invoke download
            await youtube.Videos.DownloadAsync(
                // use audio stream info from metadata
                new[] { streamInfo },
                // create conversion request
                new ConversionRequestBuilder(downloadPath)
                    // set ffmpeg output format
                    .SetFormat(format)
                    // M A X I M U M quality, highest file size xd
                    .SetPreset(ConversionPreset.UltraFast)
                    // set ffmpeg executable path
                    .SetFFmpegPath(ffmpegPath)
                    .Build(),
                progress, cancellationToken);
        }

        public async ValueTask<string> GetVideoStream(VideoId url, CancellationToken cancellationToken = default)
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url, cancellationToken);
            return streamManifest?.GetMuxedStreams().GetWithHighestVideoQuality()?.Url;
        }
    }
}
