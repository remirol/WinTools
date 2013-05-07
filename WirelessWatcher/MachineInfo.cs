using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Serialization;
using System.Xml;

namespace WirelessWatcher
{
    [Serializable]
    public class MachineInfo : IComparable, IEquatable<MachineInfo>
    {
        public String MACAddress;
        public String Description;
        public DateTime LastSeen;

        [XmlIgnore]
        public IPAddress LastIP;    // can't serialize IPAddress-es and we don't care anyway

        /// <summary>
        /// Creates a machine object with default information
        /// </summary>
        public MachineInfo() : this("00:00:00:00:00:00")
        {
        }

        /// <summary>
        /// Creates a machine object, specifying a MAC address
        /// </summary>
        /// <param name="macAddress">MAC address to assign to this machine</param>
        public MachineInfo(String macAddress)
        {
            MACAddress = macAddress;
            Description = "UNKNOWN";
            LastSeen = DateTime.MinValue;
            LastIP = new IPAddress(0xFFFFFFFF);
        }

        #region IComparable Members

        // hotwire this in so we can just compare these objects directly; MAC addresses are unique
        public int CompareTo(object obj)
        {
            return this.MACAddress.ToUpper().CompareTo(((MachineInfo)obj).MACAddress.ToUpper());
        }

        #endregion

        #region IEquatable<MachineInfo> Members

        // covers Contains();
        public bool Equals(MachineInfo other)
        {
            return (this.MACAddress.CompareTo(other.MACAddress) == 0);
        }

        #endregion
    }
}