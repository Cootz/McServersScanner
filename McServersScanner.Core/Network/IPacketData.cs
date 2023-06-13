namespace McServersScanner.Core.Network;

/// <summary>
/// Defines minecraft packet data
/// </summary>
public interface IPacketData
{
    public byte[] GetData();
}