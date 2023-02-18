using System.Net;

namespace McServersScanner.CLI
{
    /// <summary>
    /// Generates ranges of ip addresses depending on first and last addresses
    /// </summary>
    public static class IpAddressGeneratior
    {
        /// <summary>
        /// Generates ranges of ip addresses depending on first and last addresses
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ip addresses</returns>
        public static IPAddress[] FillRange(IPAddress first, IPAddress last)
        { 
            List<IPAddress> result = new List<IPAddress>();

            for (uint i = ConvertIpAddressToInt(first); i <= ConvertIpAddressToInt(last); i++)
            {
                result.Add(ConvertIntToIpAddress(i));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Generates ranges of ip addresses depending on first and last addresses
        /// </summary>
        /// <param name="first">Start of range</param>
        /// <param name="last">End of range</param>
        /// <returns>Range of ip addresses</returns>
        public static IPAddress[] FillRange(string first, string last) => FillRange(IPAddress.Parse(first), IPAddress.Parse(last));

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
