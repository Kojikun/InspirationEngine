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
        public static string ToValidFileName(this string filename) =>
            string.Join('_', filename.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
    }
}
