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
using System.Net;

namespace WirelessWatcher
{
    /// <summary>
    /// Interaction logic for CredentialsPrompt.xaml
    /// </summary>
    public partial class CredentialsPrompt : Window, INotifyPropertyChanged
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

        private String _userName;
        public String userName
        {
            get { return _userName; }
            set { _userName = value; Notify("userName"); }
        }

        private String _password;
        public String password
        {
            get { return _password; }
            set { _password = value; Notify("password"); }
        }

        public CredentialsPrompt() : this(new NetworkCredential(String.Empty, String.Empty))
        {
        }

        public CredentialsPrompt(NetworkCredential existingCreds)
        {
            InitializeComponent();

            userName = existingCreds.UserName;
            password = existingCreds.Password;
            passwordBox.Password = password;

            DataContext = this;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            password = passwordBox.Password;
            this.DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            userName = String.Empty;
            password = String.Empty;
            this.DialogResult = false;
            this.Close();
        }

    }
}
