using System.Net;

namespace McServersScanner.CLI
{
    /// <summary>
    /// Help with generating ranges of network stuff
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// Generates ranges of ip addresses depending on first and last addresses
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ip addresses</returns>
        public static IPAddress[] FillIpRange(IPAddress first, IPAddress last)
        { 
            List<IPAddress> result = new List<IPAddress>();

            for (uint i = ConvertIpAddressToInt(first); i <= ConvertIpAddressToInt(last); i++)
                result.Add(ConvertIntToIpAddress(i));

            return result.ToArray();
        }

        /// <summary>
        /// Generates ranges of ip addresses depending on first and last addresses
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ip addresses</returns>
        public static IPAddress[] FillIpRange(string first, string last) => FillIpRange(IPAddress.Parse(first), IPAddress.Parse(last));

        /// <summary>
        /// Generates range of ports depending on first and last port
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ports</returns>
        public static ushort[] FillPortRange(ushort first, ushort last)
        {
            List<ushort> result = new List<ushort>();

            for (ushort i = first; i <= last; i++)
                result.Add(i);

            return result.ToArray();
        }

        /// <summary>
        /// Generates range of ports depending on first and last port
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ports</returns>
        public static ushort[] FillPortRange(string first, string last) => FillPortRange(ushort.Parse(first), ushort.Parse(last));

        /// <summary>
        /// Converts IpAddress to int
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static uint ConvertIpAddressToInt(IPAddress address)
        { 
            return BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray());
        }

        /// <summary>
        /// Converts int to IpAddress
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static IPAddress ConvertIntToIpAddress(uint address)
        {
            return new IPAddress(BitConverter.GetBytes(address).Reverse().ToArray());
        }

    }
}
