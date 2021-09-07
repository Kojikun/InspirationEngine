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
        /// <summary>
        /// A tuple containing the names for the ffmpeg and ffprobe executables
        /// </summary>
        public static (string ffmpeg, string ffprobe) ExecutableName { get; set; }

        /// <summary>
        /// A tuple of Full Paths to the ffmpeg and ffprobe executables
        /// </summary>
        public static (string ffmpeg, string ffprobe) ExecutablePath
        {
            get =>
                (
                    Path.Combine(FFmpeg.ExecutablesPath ?? string.Empty, ExecutableName.ffmpeg ?? string.Empty),
                    Path.Combine(FFmpeg.ExecutablesPath ?? string.Empty, ExecutableName.ffprobe ?? string.Empty)
                );
        }

        /// <summary>
        /// Trims a given input file (audio) within the <paramref name="start"/> and <paramref name="end"/> TimeSpans.
        /// </summary>
        /// <param name="input">The path to the file to trim</param>
        /// <param name="output">The path to the resulting trimmed file</param>
        /// <param name="start">TimeSpan storing the amount of seconds to trim from</param>
        /// <param name="end">Timespan storing the amount of seconds to trim to</param>
        /// <param name="cancellationToken">Token that will raise a cancellation exception if canceled</param>
        /// <returns>async void</returns>
        public static async Task Trim(string input, string output, TimeSpan start, TimeSpan? end, CancellationToken cancellationToken = default)
        {
            // get audio metadata from input file
            var audioStream = (await FFmpeg.GetMediaInfo(input, cancellationToken)).AudioStreams.FirstOrDefault();

            // if not canceled
            if (!cancellationToken.IsCancellationRequested && audioStream is not null)
            {
                // start ffmpeg conversion
                await FFmpeg.Conversions.New()
                    .AddStream(audioStream)
                    // -ss to trimFrom, -to to trimTo
                    .AddParameter($"-ss {start.TotalSeconds}{(end.HasValue ? $" -to {end.Value.TotalSeconds}" : "")}")
                    // set output file
                    .SetOutput(output)
                    // start async ffmpeg process
                    .Start(cancellationToken);
            }
        }

        /// <summary>
        /// Verifies that FFmpeg can run properly
        /// </summary>
        /// <returns>Returns null if successful; returns a list of errors as a string otherwise</returns>
        public static async Task<string> Verify()
        {
            // if ExecutablesPath has not been set
            if (string.IsNullOrWhiteSpace(FFmpeg.ExecutablesPath))
            {
                return $"FFmpeg.ExecutablesPath is null; call {nameof(FFmpegInterface)}.SetExecutablesPath()";
            }
            else
            {
                // Check if both ffmpeg and ffprobe executable names have been registered
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

                            // manually invoke ffmpeg process
                            Process ffmpeg = new Process()
                            {
                                StartInfo = new ProcessStartInfo(ExecutablePath.ffmpeg)
                                {
                                    WorkingDirectory = FFmpeg.ExecutablesPath,
                                    RedirectStandardError = true
                                }
                            };

                            // start process
                            if (ffmpeg.Start())
                            {
                                // wait for output from process
                                string output = ffmpeg.StandardError.ReadToEnd();
                                ffmpeg.WaitForExit();

                                // if output does not start with "ffmpeg" use whatever is output by the process as the errorMsg
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

                // join all errors as newline-delimited string
                var totalErrors = string.Join(Environment.NewLine, errors);

                // return null if totalErros is just "ffmpeg" (which means it was successful), return error message(s) otherwise
                return !string.IsNullOrWhiteSpace(totalErrors) ? (totalErrors == "ffmpeg" ? null : totalErrors) : totalErrors;
            }
        }

        /// <summary>
        /// Sets the ExecutablesPath used by this class and by the FFmpeg library
        /// </summary>
        /// <param name="path">The path to set the ffmpeg ExecutablesPath to</param>
        /// <param name="ffmpegExeName">The name of the ffmpeg executable</param>
        /// <param name="ffprobeExeName">The name of the ffprobe executable</param>
        /// <returns></returns>
        public static async Task<bool> SetExecutablesPath(string path, string ffmpegExeName = "ffmpeg", string ffprobeExeName = "ffprobe")
        {
            var oldPath = FFmpeg.ExecutablesPath;
            var oldExes = ExecutableName;

            // attempt to set executables path
            FFmpeg.SetExecutablesPath(path, ffmpegExeName, ffprobeExeName);
            ExecutableName = (ffmpegExeName, ffprobeExeName);

            // verify that path is valid
            var errorMsg = await Verify();

            // if verify "failed" then revert 
            if (errorMsg != null)
            {
                FFmpeg.SetExecutablesPath(oldPath);
                ExecutableName = oldExes;
            }
            return FFmpeg.ExecutablesPath != oldPath;
        }

        /// <summary>
        /// Gets ffmpeg Executables Path
        /// </summary>
        /// <returns></returns>
        public static string GetExecutablesPath() =>
            FFmpeg.ExecutablesPath;
    }
}
