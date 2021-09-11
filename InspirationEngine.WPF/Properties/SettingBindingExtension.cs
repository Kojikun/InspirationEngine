using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InspirationEngine.WPF.Properties
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension() => Init();
        public SettingBindingExtension(string path) : base(path) => Init();

        private void Init()
        {
            Source = Settings.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
