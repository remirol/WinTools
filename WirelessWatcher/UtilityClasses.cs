using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WirelessWatcher
{
    /// <summary>
    /// Arguments for the "Alert Raised" event
    /// </summary>
    public class AlertEventArgs
    {
        public String MACAddress;

        public AlertEventArgs(String macAddress)
        {
            MACAddress = macAddress;
        }
    }

}
