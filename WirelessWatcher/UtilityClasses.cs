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

    /// <summary>
    /// Handles converting normal strings back and forth from an 'encrypted' form.
    /// NOTE: this is not real encryption. This is just base64 (AKA UUEncoded); its only advantage
    /// is that it is a pain for a normal human to read.  Machines can read it just fine.
    /// </summary>
    public static class CrappyCrypt
    {
        /// <summary>
        /// Turns a human-readable string into a crappy-encoded (base64) one
        /// </summary>
        /// <param name="input">A string you wish to render unreadable</param>
        /// <returns>A crappy-encoded (base64) string</returns>
        public static String Encrypt(String input)
        {
            String encryptedText = String.Empty;
            try
            {
                byte[] interimArray = Encoding.Default.GetBytes(input.ToCharArray());
                encryptedText = Convert.ToBase64String(interimArray);
            }
            catch (Exception)
            {
                // eat this and make sure we do not return junk
                encryptedText = String.Empty;
            }
            return encryptedText;
        }

        /// <summary>
        /// Turns a crappy-encoded string into a human-readable one
        /// </summary>
        /// <param name="input">A crappy-encoded (base64) string</param>
        /// <returns>The decrapped version, or String.Empty on any error</returns>
        public static String Decrypt(String input)
        {
            String decryptedText = String.Empty;
            try
            {
                byte[] interimArray = Convert.FromBase64String(input);
                decryptedText = Encoding.Default.GetString(interimArray);
            }
            catch (Exception)
            {
                return decryptedText;
            }
            return decryptedText;
        }
    }
}
