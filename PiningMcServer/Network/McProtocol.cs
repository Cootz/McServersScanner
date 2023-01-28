using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Network
{
    public static class McProtocol
    {
        const int SEGMENT_BITS = 0x7F;
        const int CONTINUE_BIT = 0x80;

        public static List<byte> WriteVarInt(int value)
        {
            List<byte> buffer = new();

            while ((value & CONTINUE_BIT) != 0)
            {
                buffer.Add((byte)(value & SEGMENT_BITS | CONTINUE_BIT));
                value = (int)((uint)value) >> 7;
            }

            buffer.Add((byte)value);
            return buffer;
        }

        public static List<byte> WriteString(string value)
        {
            byte[] valAsBytes = Encoding.UTF8.GetBytes(value);
            List<byte> buffer = new();

            buffer.AddRange(WriteVarInt(valAsBytes.Length));
            buffer.AddRange(valAsBytes);

            return buffer;
        }
    }
}
