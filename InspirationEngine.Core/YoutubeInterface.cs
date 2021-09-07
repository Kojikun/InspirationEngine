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
    public class YoutubeInterface
    {
        private YoutubeClient youtube;

        public YoutubeInterface()
        {
            youtube = new YoutubeClient();
        }

        public async IAsyncEnumerable<PlaylistVideo> GetVideosInPlaylist(PlaylistId url, bool processBatch = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (processBatch)
            {
                await foreach (var batch in youtube.Playlists.GetVideoBatchesAsync(url, cancellationToken))
                {
                    foreach (var video in batch.Items)
                    {
                        yield return video;
                    }
                }
            }
            else
            {
                await foreach (var video in youtube.Playlists.GetVideosAsync(url, cancellationToken))
                {
                    yield return video;
                }
            }
        }

        public async ValueTask<Video> GetYoutubeVideo(VideoId url, CancellationToken cancellationToken = default) =>
            await youtube.Videos.GetAsync(url, cancellationToken);

        public async IAsyncEnumerable<VideoSearchResult> SearchVideo(string query, bool processBatch = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (processBatch)
            {
                await foreach(var batch in youtube.Search.GetResultBatchesAsync(query, cancellationToken))
                {
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
                await foreach(var result in youtube.Search.GetVideosAsync(query, cancellationToken))
                {
                    yield return result;
                }
            }
        }

        public async ValueTask Download(VideoId url, string downloadPath, string format, string ffmpegPath, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url, cancellationToken);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            await youtube.Videos.DownloadAsync(
                new[] { streamInfo },
                new ConversionRequestBuilder(downloadPath)
                    .SetFormat(format)
                    .SetPreset(ConversionPreset.UltraFast)
                    .SetFFmpegPath(ffmpegPath)
                    .Build(),
                progress, cancellationToken);
        }
    }
}
