using System.Net;

namespace McServersScanner.Core.Utils
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
        public static IEnumerable<IPAddress> FillIpRange(IPAddress first, IPAddress last)
        {
            for (uint i = ConvertIpAddressToInt(first); i <= ConvertIpAddressToInt(last); i++)
                yield return ConvertIntToIpAddress(i);
        }

        /// <summary>
        /// Generates ranges of ip addresses depending on first and last addresses
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ip addresses</returns>
        public static IEnumerable<IPAddress> FillIpRange(string first, string last) => FillIpRange(IPAddress.Parse(first), IPAddress.Parse(last));

        /// <summary>
        /// Generates range of ports depending on first and last port
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ports</returns>
        public static IEnumerable<ushort> FillPortRange(ushort first, ushort last)
        {
            for (ushort i = first; i <= last; i++)
                yield return i;
        }

        /// <summary>
        /// Calculates total amount of ips in range
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Total amout of ips in range</returns>
        public static long GetIpRangeCount(IPAddress first, IPAddress last) => ConvertIpAddressToInt(last) - ConvertIpAddressToInt(first) + 1;

        /// <summary>
        /// Calculates total amount of ips in range
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Total amout of ips in range</returns>
        public static long GetIpRangeCount(string first, string last) => GetIpRangeCount(IPAddress.Parse(first), IPAddress.Parse(last));

        /// <summary>
        /// Generates range of ports depending on first and last port
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ports</returns>
        public static IEnumerable<ushort> FillPortRange(string first, string last) => FillPortRange(ushort.Parse(first), ushort.Parse(last));

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
