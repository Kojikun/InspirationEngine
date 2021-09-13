using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspirationEngine.WPF.Properties
{
    /// <summary>
    /// Application Settings defined in Settings.Settings
    /// </summary>
    /// <remarks>
    /// Settings.cs extends the partial class generated from Settings.Designer.cs
    /// </remarks>
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        public Settings()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Properties[nameof(SamplesDir)].DefaultValue = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), @"Inspiration Engine\Downloaded Samples");
        }

        /// <summary>
        /// Returns the DefaultValue for a given property
        /// </summary>
        /// <typeparam name="T">The type of the value stored within the property</typeparam>
        /// <param name="propertyName">The name of the property in <see cref="Settings"/></param>
        /// <returns>Returns the default value for the given property</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the type stored within the property for the given <paramref name="propertyName"/> is not of type <typeparamref name="T"/>,
        /// or if a property with the given <paramref name="propertyName"/> does not exist within <see cref="Settings"/>
        /// </exception>
        public T DefaultValue<T>(string propertyName)
        {
            try
            {
                return DefaultValue<T>(Properties[propertyName]);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"'{propertyName}' is not a property of {nameof(Settings)}", ex);
            }
        }

        /// <summary>
        /// Returns the DefaultValue for a given property
        /// </summary>
        /// <typeparam name="T">The type of the value stored within the property</typeparam>
        /// <param name="property">The <see cref="SettingsProperty"/> that stores information about the property</param>
        /// <returns>Returns the default vaue for the given property</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when the type stored within <paramref name="property"/> is not of type <typeparamref name="T"/></exception>
        public T DefaultValue<T>(SettingsProperty property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            if (typeof(T) != property.PropertyType)
                throw new ArgumentException($"\"{property.Name}\" is not of type {typeof(T).Name}", nameof(property));

            return (T)new SettingsPropertyValue(property).PropertyValue;
        }
    }
}
