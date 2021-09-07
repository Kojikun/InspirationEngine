using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace InspirationEngine.Core
{
    public static class FFmpegInterface
    {
        public static (string ffmpeg, string ffprobe) ExecutableName { get; set; }
        public static (string ffmpeg, string ffprobe) ExecutablePath
        {
            get =>
                (
                    Path.Combine(FFmpeg.ExecutablesPath ?? string.Empty, ExecutableName.ffmpeg ?? string.Empty),
                    Path.Combine(FFmpeg.ExecutablesPath ?? string.Empty, ExecutableName.ffprobe ?? string.Empty)
                );
        }

        public static async Task Trim(string input, string output, TimeSpan start, TimeSpan? end, CancellationToken cancellationToken = default)
        {
            var audioStream = (await FFmpeg.GetMediaInfo(input, cancellationToken)).AudioStreams.FirstOrDefault();
            if (!cancellationToken.IsCancellationRequested && audioStream is not null)
            {
                await FFmpeg.Conversions.New()
                    .AddStream(audioStream)
                    .AddParameter($"-ss {start.TotalSeconds}{(end.HasValue ? $" -to {end.Value.TotalSeconds}" : "")}")
                    .SetOutput(output)
                    .Start(cancellationToken);
            }
        }

        /// <summary>
        /// Verifies that FFmpeg can run properly
        /// </summary>
        /// <returns></returns>
        public static async Task<string> Verify()
        {
            if (string.IsNullOrWhiteSpace(FFmpeg.ExecutablesPath))
            {
                return $"FFmpeg.ExecutablesPath is null; call {nameof(FFmpegInterface)}.SetExecutablesPath()";
            }
            else
            {
                List<string> errors = new List<string>();
                if (string.IsNullOrWhiteSpace(ExecutableName.ffmpeg))
                    errors.Add($"ffmpeg executable name not set");
                if (string.IsNullOrWhiteSpace(ExecutableName.ffprobe))
                    errors.Add($"ffprobe executable name not set");

                if (errors.Count == 0)
                {
                    errors.Add(await Task.Run(() =>
                    {
                        try
                        {
                            string errorMsg = "Process did not start";
                            Process ffmpeg = new Process()
                            {
                                StartInfo = new ProcessStartInfo(ExecutablePath.ffmpeg)
                                {
                                    WorkingDirectory = FFmpeg.ExecutablesPath,
                                    RedirectStandardError = true
                                }
                            };
                            if (ffmpeg.Start())
                            {
                                string output = ffmpeg.StandardError.ReadToEnd();
                                ffmpeg.WaitForExit();
                                errorMsg = output.TrimStart().StartsWith("ffmpeg", StringComparison.InvariantCultureIgnoreCase) ? "ffmpeg" : errorMsg;
                            }

                            return errorMsg;
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }));
                }

                var totalErrors = string.Join(Environment.NewLine, errors);
                return !string.IsNullOrWhiteSpace(totalErrors) ? (totalErrors == "ffmpeg" ? null : totalErrors) : totalErrors;
            }
        }

        public static async Task<bool> SetExecutablesPath(string path, string ffmpegExeName = "ffmpeg", string ffprobeExeName = "ffprobe")
        {
            var oldPath = FFmpeg.ExecutablesPath;
            var oldExes = ExecutableName;
            FFmpeg.SetExecutablesPath(path, ffmpegExeName, ffprobeExeName);
            ExecutableName = (ffmpegExeName, ffprobeExeName);
            var errorMsg = await Verify();
            if (errorMsg != null)
            {
                FFmpeg.SetExecutablesPath(oldPath);
                ExecutableName = oldExes;
            }
            return FFmpeg.ExecutablesPath != oldPath;
        }

        public static string GetExecutablesPath() =>
            FFmpeg.ExecutablesPath;
    }
}
