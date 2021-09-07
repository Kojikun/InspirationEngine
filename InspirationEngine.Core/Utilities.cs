using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InspirationEngine.Core.Utilities
{
    public static class Utilities
    {
        /// <summary>
        /// Converts any invalid characters within the file name into underscores
        /// </summary>
        /// <param name="filename">The filename to check</param>
        /// <returns>Returns a new filename that is guarenteed to have valid characters</returns>
        public static string ToValidFileName(this string filename) =>
            string.Join('_', filename.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
    }
}
