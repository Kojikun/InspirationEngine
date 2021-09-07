using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InspirationEngine.WPF.Utilities
{
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
        /// <param name="recursive">Search the VisualTree recursively (depth-first)</param>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static T FindVisualChild<T>(this DependencyObject dependencyObject, bool recursive = true, TraversalOrder order = TraversalOrder.BreadthFirst) where T : DependencyObject =>
            dependencyObject?.FindVisualChildren<T>(recursive, order).FirstOrDefault();

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

        public static void AutoFitColumn(this DataGrid dataGrid, DataGridColumn column)
        {
            column.Width = 0;
            dataGrid.UpdateLayout();
            column.Width = DataGridLength.Auto;
        }

        public static CancellationToken RequestToken(ref CancellationTokenSource source)
        {
            if (source is not null)
            {
                source.Dispose();
            }

            source = new CancellationTokenSource();

            return source.Token;
        }

        public static bool TryGetFullPath(string path, out string result)
        {
            result = string.Empty;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            bool status = false;
            try
            {
                result = Path.GetFullPath(path);
                status = true;
            }
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return status;
        }
    }
}
