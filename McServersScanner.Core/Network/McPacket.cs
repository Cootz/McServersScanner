using System.Collections;

namespace McServersScanner.Core.Network;

/// <summary>
/// Represent minecraft packet that follows <see href="https://wiki.vg/Protocol#Packet_format">packet format</see>
/// </summary>
/// <remarks>
/// Example of usage:
/// <code>
///     McPacket packet = new McPacketImpl();
///     await networkStream.WriteAsync(packet);
/// </code>
/// </remarks>
public abstract class McPacket : IEnumerable<byte>
{
    public abstract int PacketId { get; }

    private List<byte> lengthData
    {
        get => McProtocol.WriteVarInt(data.Count);
    }

    private List<byte> packetIdData
    {
        get => McProtocol.WriteVarInt(PacketId);
    }

    protected abstract List<byte> data { get; }

    public IEnumerator<byte> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class Enumerator : IEnumerator<byte>
    {
        private State currentState = State.PacketLength;

        private readonly IEnumerator<byte> packetLengthEnumerator;
        private readonly IEnumerator<byte> packetIdEnumerator;
        private readonly IEnumerator<byte> packetDataEnumerator;

        public Enumerator(McPacket packet)
        {
            packetDataEnumerator = packet.data.GetEnumerator();
            packetIdEnumerator = packet.packetIdData.GetEnumerator();
            packetLengthEnumerator = packet.lengthData.GetEnumerator();
        }

        public bool MoveNext()
        {
            //Doing || here is fine because it will not evaluate second operand until first operand value is true (see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators#conditional-logical-or-operator)
            return currentState switch
            {
                State.PacketLength => packetLengthEnumerator.MoveNext() || changeStateAndRerun(State.PacketId),
                State.PacketId => packetIdEnumerator.MoveNext() || changeStateAndRerun(State.PacketData),
                State.PacketData => packetDataEnumerator.MoveNext(),
                _ => false
            };

            bool changeStateAndRerun(State newState)
            {
                currentState = newState;

                return MoveNext();
            }
        }

        public void Reset()
        {
            currentState = State.PacketLength;

            packetLengthEnumerator.Reset();
            packetDataEnumerator.Reset();
        }

        public byte Current
        {
            get => currentState switch
            {
                State.PacketLength => packetLengthEnumerator.Current,
                State.PacketId => packetIdEnumerator.Current,
                State.PacketData => packetDataEnumerator.Current,
                _ => throw new ArgumentOutOfRangeException(nameof(State), "Argument out of range")
            };
        }

        object IEnumerator.Current
        {
            get => Current;
        }

        public void Dispose()
        {
            packetLengthEnumerator.Dispose();
            packetDataEnumerator.Dispose();
        }

        private enum State
        {
            PacketLength,
            PacketId,
            PacketData
        }
    }
}