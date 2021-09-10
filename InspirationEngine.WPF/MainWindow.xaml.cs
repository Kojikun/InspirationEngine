using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using static InspirationEngine.WPF.Utilities.Utilities;

namespace InspirationEngine.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            if (!TryGetFullPath(Properties.Settings.Default.SamplesDir, out _))
            {
                Properties.Settings.Default.SamplesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), @"Inspiration Engine\Downloaded Samples");
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Invoked when something within the MainWindow invokes the CloseCommand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Invoked when Window.Close() has been called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Closing Application...");
            e.Cancel = false;
        }
    }
}
