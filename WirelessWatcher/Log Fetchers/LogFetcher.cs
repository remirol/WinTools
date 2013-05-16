using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace WirelessWatcher
{
    /// <summary>
    /// Base class for individual router log fetchers
    /// </summary>
    public abstract class LogFetcher
    {
        #region Variables/Properties

        /// <summary>
        /// Retrieved logfile text to be parsed, formatted in a list of lines
        /// </summary>
        public abstract List<String> LogText { get; }

        /// <summary>
        /// The device we're obtaining this log from
        /// </summary>
        public abstract String DeviceName { get; }

        /// <summary>
        /// A regex matching string that will pick out the appropriate pieces from this manufacturer's log
        /// </summary>
        public abstract String RegexMatchString { get; }

        /// <summary>
        /// Any login credentials needed to fetch the log
        /// </summary>
        public abstract NetworkCredential loginCredentials { get; }

        #endregion

        /// <summary>
        /// Does what is necessary to obtain the text of a logfile to parse
        /// </summary>
        /// <returns>Whether the request was successful</returns>
        public abstract bool Fetch();

        /// <summary>
        /// Displays any UI necessary to prompt the user for credentials, and stores them in loginCredentials
        /// </summary>
        /// <returns>Whether credentials were successfully obtained</returns>
        public abstract bool PromptCredentials();

    }
}
