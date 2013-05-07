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
using NetgearLogParser.Properties;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Xml;

namespace NetgearLogParser
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

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            allMachines = new List<MachineInfo>();
            knownMachines = new List<MachineInfo>();
            ReadKnownMachines();
            fileToParse = Settings.Default.lastUsedLogFile;
            IsDirty = false;
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
                allMachines.Clear();

                // note: this is specific to WNDR4000, should eventually be put into table of routers to select from
                Regex matchString = new Regex(@"\[DHCP IP: \((.*)\)\] to MAC address ([\d\:abcdef]+), (.*)", RegexOptions.IgnoreCase);
                StreamReader reader = File.OpenText(fileToParse);
                String line = reader.ReadLine();
                while (!String.IsNullOrEmpty(line))
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
                    line = reader.ReadLine();
                }
                reader.Close();
                statusText = "Logfile loaded successfully.";
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
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(fileToParse);    // start at the last place we were
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
            EditDetails dlg = new EditDetails(selectedMAC);
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
