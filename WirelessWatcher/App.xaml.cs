using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using NetgearLogParser.Properties;

namespace NetgearLogParser
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
            Settings.Default.lastUsedLogFile = Settings.Default.GetPreviousVersion("lastUsedLogFile").ToString();
            Settings.Default.Save();
        }
    }
}
