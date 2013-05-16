using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using WirelessWatcher.Properties;

namespace WirelessWatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // make sure any settings we add both stay up to date and don't trash older versions
            Settings.Default.Upgrade();
            Settings.Default.lastUsedLogFile = (String)Settings.Default.GetPreviousVersion("lastUsedLogFile");
            Settings.Default.savedCreds = (String)Settings.Default.GetPreviousVersion("savedCreds");
            Settings.Default.Save();
        }
    }
}
