﻿namespace McServersScanner.Core.IO;

public class SharedThrottledStream : Stream
{
    private readonly Stream parent;

    private readonly IThrottleManager throttleManager;

    public SharedThrottledStream(Stream parent, IThrottleManager throttleManager)
    {
        this.parent = parent;
        this.throttleManager = throttleManager;
    }

    public void Throttle(int bytes)
    {
        ThrottleAsync(bytes).GetAwaiter().GetResult();
    }

    public async Task ThrottleAsync(int bytes)
    {
        if (bytes > throttleManager.MaxBytesPerSecond)
            throw new ArgumentOutOfRangeException(nameof(bytes),
                "Buffer size cannot be larger than bandwidth per second");

        await throttleManager.Throttle(bytes);
    }

    public override bool CanRead
    {
        get => parent.CanRead;
    }

    public override bool CanSeek
    {
        get => parent.CanSeek;
    }

    public override bool CanWrite
    {
        get => parent.CanWrite;
    }

    public override void Flush()
    {
        parent.Flush();
    }

    public override long Length
    {
        get => parent.Length;
    }

    public override long Position
    {
        get => parent.Position;
        set => parent.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = parent.Read(buffer, offset, count);
        Throttle(read);
        return read;
    }

    public override int ReadByte()
    {
        int read = parent.ReadByte();
        Throttle(1);
        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int read = await parent.ReadAsync(buffer, offset, count, cancellationToken);
        await ThrottleAsync(read);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = new())
    {
        int read = await parent.ReadAsync(buffer, cancellationToken);
        await ThrottleAsync(read);
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin) => parent.Seek(offset, origin);

    public override void SetLength(long value) => parent.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        Throttle(count);
        parent.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await ThrottleAsync(count);
        await parent.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = new())
    {
        await ThrottleAsync(buffer.Length);
        await parent.WriteAsync(buffer, cancellationToken);
    }
}