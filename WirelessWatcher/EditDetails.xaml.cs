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
using System.Windows.Shapes;
using System.ComponentModel;

namespace WirelessWatcher
{
    /// <summary>
    /// Interaction logic for EditDetails.xaml
    /// </summary>
    public partial class EditDetails : Window, INotifyPropertyChanged
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

        #region props, vars, and stuff

        private String _macAddress;
        public String macAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; Notify("macAddress"); }
        }

        private String _description;
        public String description
        {
            get { return _description; }
            set { _description = value; Notify("description"); }
        }

        #endregion

        public EditDetails(String MACaddress)
        {
            InitializeComponent();

            _macAddress = MACaddress;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Notify("macAddress");   // cheat
            newDescription.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
