using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Net;
using WirelessWatcher.Properties;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Xml;
using SWF = System.Windows.Forms;
using System.Reflection;
using WirelessWatcher.Log_Fetchers;

namespace WirelessWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(String propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        #region props, vars, and crap

        private ObservableCollection<String> _messages;
        /// <summary>
        /// Displayable list of messages
        /// </summary>
        public ObservableCollection<String> messages
        {
            get
            {
                if (_messages == null)
                {
                    _messages = new ObservableCollection<string>();
                }
                return _messages;
            }
            set { _messages = value; Notify("messages"); }
        }

        private String _fileToParse;
        /// <summary>
        /// Logfile to read info from
        /// </summary>
        public String fileToParse
        {
            get { return _fileToParse; }
            set { _fileToParse = value; Notify("fileToParse"); }
        }

        private String _statusText;
        /// <summary>
        /// Status bar
        /// </summary>
        public String statusText
        {
            get { return _statusText; }
            set { _statusText = value; Notify("statusText"); }
        }

        /// <summary>
        /// All machines that were present in the parsed log
        /// </summary>
        private List<MachineInfo> allMachines;

        /// <summary>
        /// Our list of "known" MAC addresses with descriptions
        /// </summary>
        private List<MachineInfo> knownMachines;

        /// <summary>
        /// whether we've made changes to the "known MAC" list
        /// </summary>
        private bool IsDirty;

        /// <summary>
        /// Our tray icon to hang notifications from
        /// </summary>
        private SWF.NotifyIcon trayIcon;

        #endregion

        #region Events, Timers

        /// <summary>
        /// Triggers when an "Unknown" MAC address is detected
        /// </summary>
        public event AlertRaisedEventHandler AlertRaised;
        public delegate void AlertRaisedEventHandler(object sender, AlertEventArgs e);

        private SWF.Timer refreshTimer;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            AlertRaised += new AlertRaisedEventHandler(MainWindow_AlertRaised);

            allMachines = new List<MachineInfo>();
            knownMachines = new List<MachineInfo>();
            fileToParse = Settings.Default.lastUsedLogFile;
            IsDirty = false;

            refreshTimer = new SWF.Timer();
            refreshTimer.Interval = 3600 * 1000;    // value in ms; start from every hour
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            refreshTimer.Enabled = true;
            refreshTimer.Start();

            ReadKnownMachines();
            BuildTrayIcon();
        }

        /// <summary>
        /// reread log on every tick
        /// </summary>
        void refreshTimer_Tick(object sender, EventArgs e)
        {
            ParseLog();
        }

        /// <summary>
        /// Handle notifying the user if something fishy is going on
        /// </summary>
        /// <param name="e">The MAC address</param>
        void MainWindow_AlertRaised(object sender, AlertEventArgs e)
        {
            trayIcon.BalloonTipIcon = SWF.ToolTipIcon.Warning;
            trayIcon.BalloonTipTitle = "Unrecognized MAC address connected!";
            trayIcon.BalloonTipText = String.Format("The MAC address {0} just connected to your network.", e.MACAddress);
            trayIcon.ShowBalloonTip(30000);
        }

        /// <summary>
        ///  Wrapper to raise an alert for a bad MAC address
        /// </summary>
        /// <param name="mac">MAC address to alert the user for</param>
        void RaiseAlert(String mac)
        {
            AlertRaisedEventHandler handler = AlertRaised;
            if (handler != null)
            {
                handler(this, new AlertEventArgs(mac));
            }
        }

        /// <summary>
        /// initial configuration/setup for our tray icon
        /// </summary>
        private void BuildTrayIcon()
        {
            trayIcon = new SWF.NotifyIcon();
            trayIcon.Click += new EventHandler(trayIcon_Click);
            trayIcon.BalloonTipClicked += new EventHandler(trayIcon_BalloonTipClicked);
            trayIcon.Icon = Properties.Resources.skepticalKitty;
            trayIcon.Text = "WirelessWatcher";
            trayIcon.Visible = true;
        }

        void trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // always display the app if the user is clicking our warning
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
        }

        void trayIcon_Click(object sender, EventArgs e)
        {
            switch (this.Visibility)
            {
                case Visibility.Collapsed:
                    this.Visibility = Visibility.Hidden;
                    break;
                case Visibility.Hidden:
                    // at least here, try to restore the damn thing
                    this.WindowState = WindowState.Normal;
                    this.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    this.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void readLogButton_Click(object sender, RoutedEventArgs e)
        {
            ParseLog();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ParseLog();
        }

        /// <summary>
        /// Open the logfile and read everything out of it
        /// </summary>
        private void ParseLog()
        {
            try
            {
                if (String.IsNullOrEmpty(fileToParse))
                {
                    statusText = "You need to select an actual logfile to parse first.";
                    return;
                }

                allMachines.Clear();
                NetgearWNDR4000 logUtil = new NetgearWNDR4000(fileToParse);
                Regex matchString = new Regex(logUtil.RegexMatchString, RegexOptions.IgnoreCase);
                if (logUtil.Fetch())
                {
                    foreach (String line in logUtil.LogText)
                    {
                        // don't puke on a mismatch
                        try
                        {
                            Match match = matchString.Match(line);
                            if (match.Success)
                            {
                                MachineInfo mInfo = new MachineInfo(match.Groups[2].Value);
                                DateTime.TryParse(match.Groups[3].Value, out mInfo.LastSeen);
                                IPAddress.TryParse(match.Groups[1].Value, out mInfo.LastIP);
                                if (!allMachines.Contains(mInfo))
                                {
                                    // we can cheat and reuse 'equals' here since the prototype matches what we need in a predicate
                                    MachineInfo knownBox = knownMachines.Find(mInfo.Equals);
                                    if (knownBox != null)
                                    {
                                        mInfo.Description = knownBox.Description;
                                        knownBox.LastSeen = mInfo.LastSeen;
                                        knownBox.LastIP = mInfo.LastIP;
                                    }
                                    else
                                    {
                                        RaiseAlert(mInfo.MACAddress);
                                    }
                                    allMachines.Add(mInfo);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // don't crash, just report the error
                            messages.Add(ex.Message);
                        }
                    }
                    statusText = String.Format("Logfile loaded successfully; last update {0}.", DateTime.Now.GetDateTimeFormats('u')[0]);
                }
                else
                {
                    statusText = "Unable to parse logfile";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error reading log", MessageBoxButton.OK, MessageBoxImage.Error);
                statusText = "Unable to parse logfile";
            }

            UpdateMachineList();
        }

        /// <summary>
        /// Update the visible list of machines
        /// </summary>
        private void UpdateMachineList()
        {
            messages.Clear();
            foreach (MachineInfo mInfo in allMachines)
            {
                // use the sortable date format here for later
                messages.Add(String.Format("{0}  {1} -- {2}", mInfo.MACAddress, mInfo.LastSeen.GetDateTimeFormats('u')[0], mInfo.Description));
            }
        }

        /// <summary>
        /// Select the logfile to be read
        /// </summary>
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.AddExtension = false;
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            if (!String.IsNullOrEmpty(fileToParse))
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(fileToParse);    // start at the last place we were
            }
            dlg.Title = "Select the log file to parse";
            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
            {
                fileToParse = dlg.FileName;
                ParseLog();
            }
        }

        /// <summary>
        /// Deserialize the "known machines" list from the designated spot on disk
        /// </summary>
        private void ReadKnownMachines()
        {
            try
            {
                String storageFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sapphire\\KnownMACs.xml");
                if (File.Exists(storageFile))
                {
                    XmlSerializer ser = new XmlSerializer(knownMachines.GetType());
                    XmlReader reader = XmlReader.Create(storageFile);
                    knownMachines = (List<MachineInfo>)ser.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error reading known-MAC table", MessageBoxButton.OK, MessageBoxImage.Error);
                statusText = "Error reading known-MAC table";
            }
        }

        /// <summary>
        /// Save off the "known machines" list to disk
        /// </summary>
        private void SaveKnownMachines()
        {
            try
            {
                // don't bother with this if we didn't mark any as 'known'
                if (IsDirty)
                {
                    String storageFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sapphire");
                    if (!Directory.Exists(storageFile))
                    {
                        Directory.CreateDirectory(storageFile);
                    }
                    storageFile += "\\KnownMACs.xml";
                    XmlSerializer ser = new XmlSerializer(knownMachines.GetType());
                    XmlWriter writer = XmlWriter.Create(storageFile);
                    ser.Serialize(writer, knownMachines);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error updating known-MAC table", MessageBoxButton.OK, MessageBoxImage.Error);
                statusText = "Error writing known-MAC table";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // save off our stuff
            Settings.Default.lastUsedLogFile = fileToParse;
            Settings.Default.Save();

            SaveKnownMachines();
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }

        /// <summary>
        /// Allow the user to add a description for a MAC address they recognize
        /// </summary>
        private void markButton_Click(object sender, RoutedEventArgs e)
        {
            if (messageList.SelectedIndex < 0)
            {
                statusText = "You must select an entry before you can mark something as known.";
                return;
            }

            if (!messageList.SelectedItem.ToString().Contains("UNKNOWN"))
            {
                statusText = "You've already marked this entry as known.";
                return;
            }

            // find out who the user thinks this is
            String selectedMAC = messageList.SelectedItem.ToString().Substring(0, 17);
            EditDetails dlg = new EditDetails(this, selectedMAC);
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                // piggyback on this since it matches what we need
                MachineInfo foundBox = allMachines.Find(new MachineInfo(selectedMAC).Equals);
                foundBox.Description = dlg.description;
                knownMachines.Add(foundBox);
                IsDirty = true;
                UpdateMachineList();

                // put us back where we were in case someone is using the arrow keys
                messageList.Focus();
                messageList.SelectedIndex = allMachines.FindIndex(foundBox.Equals);
            }
        }
    }

}
