using System.Text;

namespace McServersScanner.Core.Network;

/// <summary>
/// Provide some types from minecraft protocol
/// </summary>
public static class McProtocol
{
    private const int SEGMENT_BIT = 0x7F;
    private const int CONTINUE_BIT = 0x80;

    /// <summary>
    /// Creates varInt
    /// </summary>
    /// <returns>
    /// <see cref="List{byte}"/> representing varInt in protocol
    /// </returns>
    public static List<byte> WriteVarInt(int value)
    {
        List<byte> buffer = new();

        while ((value & CONTINUE_BIT) != 0)
        {
            buffer.Add((byte)((value & SEGMENT_BIT) | CONTINUE_BIT));
            value = (int)(uint)value >> 7;
        }

        buffer.Add((byte)value);
        return buffer;
    }

    /// <summary>
    /// Creates string
    /// </summary>
    /// <returns>
    /// <see cref="List{byte}"/> representing string in protocol
    /// </returns>
    public static List<byte> WriteString(string value)
    {
        byte[] valAsBytes = Encoding.UTF8.GetBytes(value);
        List<byte> buffer = new();

        buffer.AddRange(WriteVarInt(valAsBytes.Length));
        buffer.AddRange(valAsBytes);

        return buffer;
    }
}