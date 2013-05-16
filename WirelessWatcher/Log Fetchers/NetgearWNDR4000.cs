using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Net;
using WirelessWatcher.Properties;

namespace WirelessWatcher.Log_Fetchers
{
    public class NetgearWNDR4000 : LogFetcher
    {
        const String pageURL = "fwLog.cgi";
        String fileToParse = String.Empty;

        public NetgearWNDR4000(String file)
        {
            fileToParse = file;
            if (Settings.Default.savedCreds != null && !String.IsNullOrEmpty(Settings.Default.savedCreds))
            {
                string[] creds = CrappyCrypt.Decrypt(Settings.Default.savedCreds).Split('|');
                _loginCredentials = new NetworkCredential(creds[0], creds[1]);
            }
            else
            {
                _loginCredentials = new NetworkCredential(String.Empty, String.Empty);
            }
        }

        private List<string> _logText;
        public override List<string> LogText
        {
            get 
            { 
                if (_logText == null) 
                { 
                    _logText = new List<string>(); 
                } 
                return _logText; 
            }
        }

        public override string DeviceName
        {
            get { return "WNDR4000"; }
        }

        public override string RegexMatchString
        {
            get { return @"\[DHCP IP: \((.*)\)\] to MAC address ([\d\:abcdef]+), (.*)"; }
        }

        private NetworkCredential _loginCredentials;
        public override NetworkCredential loginCredentials 
        {
            get { return _loginCredentials; }
        }

        public override bool Fetch()
        {
            bool success = false;
            try
            {
                WebClient client = new WebClient();
                CredentialCache mycache = new CredentialCache();
                if (loginCredentials == null || String.IsNullOrEmpty(loginCredentials.UserName) || String.IsNullOrEmpty(loginCredentials.Password))
                {
                    if (!PromptCredentials())
                    {
                        return false;
                    }
                }
                mycache.Add(new Uri(fileToParse), "Basic", loginCredentials);
                client.Credentials = mycache;
                StreamReader reader = new StreamReader(client.OpenRead(new Uri(fileToParse)));
                String line = reader.ReadLine();
                bool bLogTextStarted = false;
                while (line != null)
                {
                    if (line.ToLower().Contains("</textarea>"))
                    {
                        bLogTextStarted = false;
                    }
                    if (bLogTextStarted)
                    {
                        LogText.Add(line);
                    }
                    if (line.ToLower().Contains("<textarea"))
                    {
                        bLogTextStarted = true;
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return success;
        }

        public override bool PromptCredentials()
        {
            CredentialsPrompt credDlg = new CredentialsPrompt();
            credDlg.ShowDialog();
            if (credDlg.DialogResult == true)
            {
                _loginCredentials = new NetworkCredential(credDlg.userName, credDlg.password);
                Settings.Default.savedCreds = CrappyCrypt.Encrypt(String.Format("{0}|{1}", credDlg.userName, credDlg.password));
                Settings.Default.Save();
            }
            return (credDlg.DialogResult == true);
        }
    }
}
