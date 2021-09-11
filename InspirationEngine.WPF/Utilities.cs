using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InspirationEngine.WPF.Utilities
{
    /// <summary>
    /// Simple enumeration to define the traversal order of a recursive call
    /// </summary>
    public enum TraversalOrder
    {
        BreadthFirst,
        DepthFirst
    }

    /// <summary>
    /// Extension methods relevant to the project
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Returns a list of <typeparamref name="T"/> that are descendants of <paramref name="dependencyObject"/> on the VisualTree
        /// </summary>
        /// <typeparam name="T">Child DependencyObject type</typeparam>
        /// <param name="dependencyObject">The DependencyObject to search for children from</param>
        /// <param name="recursive">Search the VisualTree recursively</param>
        /// <param name="order">The traversal order to search the visual tree</param>
        /// <returns>Returns a lazily-evaluated list of <typeparamref name="T"/> that are descendants of <paramref name="dependencyObject"/></returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject dependencyObject, bool recursive = true, TraversalOrder order = TraversalOrder.DepthFirst) where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                if (!recursive || order == TraversalOrder.DepthFirst)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); ++i)
                    {
                        var child = VisualTreeHelper.GetChild(dependencyObject, i);
                        if (child is not null and T typedChild)
                        {
                            yield return typedChild;
                        }

                        // Recursive DFS
                        if (recursive)
                        {
                            foreach (T grandchild in FindVisualChildren<T>(child))
                            {
                                yield return grandchild;
                            }
                        }
                    }
                }
                // Recursive BFS
                else
                {
                    var directDescendants = dependencyObject.FindVisualChildren<DependencyObject>(false);
                    foreach (var child in directDescendants.OfType<T>().Concat(directDescendants.SelectMany(child => child.FindVisualChildren<T>(true, TraversalOrder.BreadthFirst))))
                        yield return child;
                }
            }    
        }

        /// <summary>
        /// Returns the first instance of a <typeparamref name="T"/> that is a descendant of 
        /// </summary>
        /// <typeparam name="T">Child DependencyObject type</typeparam>
        /// <param name="dependencyObject">The DependencyObject to search for a child from</param>
        /// <param name="recursive">Search the VisualTree recursively (breadth-first)</param>
        /// <returns>Returns the first VisualChild of the given type, or null if none was found</returns>
        public static T FindVisualChild<T>(this DependencyObject dependencyObject, bool recursive = true, TraversalOrder order = TraversalOrder.BreadthFirst) where T : DependencyObject =>
            dependencyObject?.FindVisualChildren<T>(recursive, order).FirstOrDefault();

        /// <summary>
        /// Traverse the VisualTree upwards to find a parent of a given type
        /// </summary>
        /// <typeparam name="T">Parent DependencyObject type</typeparam>
        /// <param name="dependencyObject">The DependencyObject to search for parents from</param>
        /// <param name="recursive">Whether or not to search parents of parents</param>
        /// <returns>Returns the first parent of a given type, or null if none was found</returns>
        public static T FindParent<T>(this DependencyObject dependencyObject, bool recursive = true) where T : DependencyObject
        {
            if (dependencyObject is null)
                return null;

            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent is T typedParent)
                return typedParent;

            if (recursive)
                return parent.FindParent<T>();
            else
                return null;
        }

        /// <summary>
        /// Autofit the column width based off of its contents
        /// </summary>
        /// <param name="dataGrid">The DataGrid that contains the given column</param>
        /// <param name="column">The column to auto-size</param>
        public static void AutoFitColumn(this DataGrid dataGrid, DataGridColumn column)
        {
            column.Width = 0;
            dataGrid.UpdateLayout();
            column.Width = DataGridLength.Auto;
        }

        /// <summary>
        /// Resets the state of a CancellationTokenSource and requests a brand new token
        /// </summary>
        /// <param name="source">The CancellationTokenSource to dispose and recreate</param>
        /// <returns>Returns a shiny new token from the shiny new CancellationTokenSource</returns>
        public static CancellationToken RequestToken(ref CancellationTokenSource source)
        {
            // dispose source
            if (source is not null)
            {
                source.Dispose();
            }

            // reinstantiate sourece
            source = new CancellationTokenSource();

            // return brand-spankin new token
            return source.Token;
        }

        /// <summary>
        /// Attempt to get the full path for a given filename
        /// </summary>
        /// <param name="path">The filename to get the full path from</param>
        /// <param name="result">The resulting path</param>
        /// <returns>Returns true if a full path was able to be made</returns>
        /// <remarks>Can be used to get the validity of a path</remarks>
        public static bool TryGetFullPath(string path, out string result)
        {
            // no need to waste processing power for a new exception if we can just check for empty string
            result = string.Empty;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            bool status = false;
            // attempt to get full path
            try
            {
                result = Path.GetFullPath(path);
                status = true;
            }
            // if you get caught here, you suck lmao
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return status;
        }

        /// <summary>
        /// Converts any invalid characters within the file name into underscores
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <returns>Returns a new filename that is guarenteed to have valid characters</returns>
        public static string ToValidFileName(this string filename) =>
            string.Join('_', filename.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Returns a fully qualified file path that is guarenteed to not exist
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>Returns a fully qualified path that leads to a file that has yet to exist</returns>
        public static string EnsureUniqueFile(this string path)
        {
            var fullPath = Path.GetFullPath(path);
            int i = 1;
            var og_filename = Path.GetFileNameWithoutExtension(fullPath);

            // if file already exists
            while (File.Exists(fullPath))
            {
                var dirParts = (
                    dir: Path.GetDirectoryName(fullPath),
                    file: Path.GetFileNameWithoutExtension(fullPath),
                    ext: Path.GetExtension(fullPath));
                dirParts.file = $"{og_filename} ({i++})";
                // Filename will now be "dir\path (i).ext" kinda like how Windows deals with copies
                fullPath = Path.Combine(dirParts.dir, $"{dirParts.file}{dirParts.ext}");
            }

            return fullPath;
        }

        public static SettingsProperty Setting(string propertyName) =>
            Properties.Settings.Default.Properties[propertyName];

        public static async Task<bool> IsAccessibleAsync(this Uri uri)
        {
            string url = uri?.AbsoluteUri;
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(uri));

            if (url.IndexOf(':') < 0)
                url = $"http://{url.TrimStart('/')}";

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return false;

            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request is null)
                return false;
            request.Method = "HEAD";

            try
            {
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response?.StatusCode == HttpStatusCode.OK)
                        return true;

                    return false;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
