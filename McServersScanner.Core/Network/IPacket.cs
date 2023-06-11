namespace McServersScanner.Core.Network
{
    /// <summary>
    /// Defines minecraft packet data
    /// </summary>
    public interface IPacket
    {
        public byte[] GetData();
    }
}
