using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspirationEngine.WPF.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        public Settings()
        {
            Properties[nameof(SamplesDir)].DefaultValue = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), @"Inspiration Engine\Downloaded Samples");
        }
        public T DefaultValue<T>(SettingsProperty property)
        {
            if (typeof(T) == property.PropertyType)
                return (T)new SettingsPropertyValue(property).PropertyValue;
            throw new ArgumentException($"\"{property.Name}\" is not of type {typeof(T).Name}", nameof(property));
        }
    }
}
