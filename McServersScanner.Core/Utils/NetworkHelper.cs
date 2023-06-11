using System.Net;

namespace McServersScanner.Core.Utils;

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
        for (uint i = convertIpAddressToInt(first); i <= convertIpAddressToInt(last); i++)
            yield return convertIntToIpAddress(i);
    }

    /// <summary>
    /// Generates ranges of ip addresses depending on first and last addresses
    /// </summary>
    /// <param name="first">Start of range</param>
    /// <param name="last">End of range</param>
    /// <returns>Range of ip addresses</returns>
    public static IEnumerable<IPAddress> FillIpRange(string first, string last) =>
        FillIpRange(IPAddress.Parse(first), IPAddress.Parse(last));

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
    public static long GetIpRangeCount(IPAddress first, IPAddress last) =>
        convertIpAddressToInt(last) - convertIpAddressToInt(first) + 1;

    /// <summary>
    /// Calculates total amount of ips in range
    /// </summary>
    /// <param name="first">Start of range</param>
    /// <param name="last">End of range</param>
    /// <returns>Total amout of ips in range</returns>
    public static long GetIpRangeCount(string first, string last) =>
        GetIpRangeCount(IPAddress.Parse(first), IPAddress.Parse(last));

    /// <summary>
    /// Generates range of ports depending on first and last port
    /// </summary>
    /// <param name="first">Start of range</param>
    /// <param name="last">End of range</param>
    /// <returns>Range of ports</returns>
    public static IEnumerable<ushort> FillPortRange(string first, string last) =>
        FillPortRange(ushort.Parse(first), ushort.Parse(last));

    /// <summary>
    /// Converts IpAddress to int
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private static uint convertIpAddressToInt(IPAddress address) =>
        BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray());

    /// <summary>
    /// Converts int to IpAddress
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private static IPAddress convertIntToIpAddress(uint address) =>
        new(BitConverter.GetBytes(address).Reverse().ToArray());
}