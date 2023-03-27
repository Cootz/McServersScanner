using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner
{
    /// <summary>
    /// Provide variables for using various scanner options
    /// </summary>
    public class ScannerConfiguration
    {
        /// <summary>
        /// Block of Ips to scan
        /// </summary>
        public BufferBlock<IPAddress> ips = null!;

        /// <summary>
        /// Array of ports to scan
        /// </summary>
        public ushort[]? ports = null;

        /// <summary>
        /// Amout of active connections app can handle at the same time
        /// </summary>
        public int? connectionLimit = null;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public double? timeout = null;

        /// <summary>
        /// Supplies <see cref="ips" with <see cref="IPAddress"/>es/>
        /// </summary>
        public Task? addIpAdresses = null;

        /// <summary>
        /// Amount of ips to scan
        /// </summary>
        public long? totalIps = null;
    }
}
