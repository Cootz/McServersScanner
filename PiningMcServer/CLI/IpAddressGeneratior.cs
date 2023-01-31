using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.CLI
{
    public static class IpAddressGeneratior
    {
        public static IPAddress[] FillRange(IPAddress first, IPAddress last)
        { 
            List<IPAddress> result = new List<IPAddress>();

            for (uint i = ConvertIpAddressToInt(first); i <= ConvertIpAddressToInt(last); i++)
            {
                result.Add(ConvertIntToIpAddress(i));
            }

            return result.ToArray();
        }

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

        private static IPAddress ConvertIntToIpAddress(uint address)
        {
            return new IPAddress(BitConverter.GetBytes(address).Reverse().ToArray());
        }

    }
}
