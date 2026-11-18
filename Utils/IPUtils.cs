using System.Net;

namespace CustomOverlayFramework.Utils
{
    public class IPUtils
    {
        /// <summary>
        /// Return the IP Address of the string if it is one, else return loopback address
        /// </summary>
        /// <param name="ip">IP Adress</param>
        /// <returns>IPAdress object of the IP Adress if valid or fallback to Loopback IP</returns>
        public static IPAddress TryValid(string ip)
        {
            // 127.0.0.1
            IPAddress validadress = IPAddress.Loopback;
            if(IPAddress.TryParse(ip, out IPAddress address))
            {
                validadress = address;
            }

            return validadress;
        }

        /// <summary>
        /// Normalize the IP Address to IPV4
        /// </summary>
        /// <param name="ip">IP Adress</param>
        /// <returns>IPV4 version of IP</returns>
        public static IPAddress Normalize(IPAddress ip)
        {
            IPAddress normalizedIp = ip;
            if(ip.IsIPv4MappedToIPv6)
            {
                normalizedIp = ip.MapToIPv4();
            }

            return normalizedIp;
        }
    }
}
