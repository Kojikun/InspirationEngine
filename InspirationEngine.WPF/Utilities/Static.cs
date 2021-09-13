using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InspirationEngine.WPF.Utilities
{
    /// <summary>
    /// Provides various helper static methods
    /// </summary>
    /// <remarks>
    /// using static InspirationEngine.WPF.Utilities.Static to access these methods easily
    /// </remarks>
    public static class Static
    {
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
        /// Shorthand for returning a <see cref="SettingsProperty"/> from <see cref="Properties.Settings.Default"/>
        /// </summary>
        /// <param name="propertyName">The name of a Property defined in <see cref="Properties.Settings"/></param>
        /// <returns>Returns a <see cref="SettingsProperty"/> for a given setting name</returns>
        public static SettingsProperty Setting(string propertyName) =>
            Properties.Settings.Default.Properties[propertyName];
    }
}
