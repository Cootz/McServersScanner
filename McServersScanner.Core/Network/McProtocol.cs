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
    /// Reads varInt from the <see cref="Stream"/>
    /// </summary>
    /// <returns>
    /// <see cref="int"/> representation of varInt
    /// </returns>
    public static int ReadVarInt(Stream input)
    {
        int value = 0;
        int position = 0;

        while (true)
        {
            byte currentByte = (byte)input.ReadByte();

            value |= (currentByte & segment_bit) << position;

            if ((currentByte & continue_bit) == 0) break;

            position += 7;

            if (position >= 32) throw new IndexOutOfRangeException("Var int is too long");
        }

        return value;
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

    /// <summary>
    /// Asynchronously read <see cref="string"/> from <paramref name="input"/>
    /// </summary>
    /// <returns>
    /// <see cref="string"/> extracted from <paramref name="input"/>
    /// </returns>
    public static async Task<string> ReadStringAsync(Stream input)
    {
        int length = ReadVarInt(input);

        byte[] buffer = new byte[length];

        _ = await input.ReadAsync(buffer);

        string result = Encoding.Default.GetString(buffer);

        return result;
    }
}