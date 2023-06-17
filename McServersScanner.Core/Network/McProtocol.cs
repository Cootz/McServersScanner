using System.Text;

namespace McServersScanner.Core.Network;

/// <summary>
/// Provide some types from minecraft protocol
/// </summary>
public static class McProtocol
{
    private const int segment_bit = 0x7F;
    private const int continue_bit = 0x80;

    /// <summary>
    /// Creates varInt
    /// </summary>
    /// <returns>
    /// <see cref="List{T}"/> representing varInt in protocol
    /// </returns>
    public static List<byte> WriteVarInt(int value)
    {
        List<byte> buffer = new();

        while ((value & continue_bit) != 0)
        {
            buffer.Add((byte)((value & segment_bit) | continue_bit));
            value = (int)(uint)value >> 7;
        }

        buffer.Add((byte)value);
        return buffer;
    }

    /// <summary>
    /// Creates string
    /// </summary>
    /// <returns>
    /// <see cref="List{T}"/> representing string in protocol
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