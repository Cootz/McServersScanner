namespace McServersScanner.Core.Network;

internal static class WaitHandleHelper
{
    public static Task WaitOneAsync(this WaitHandle waitHandle)
    {
        if (waitHandle == null)
            throw new ArgumentNullException("waitHandle");

        TaskCompletionSource<bool> tcs = new();

        RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
            delegate { tcs.TrySetResult(true); }, null, -1, true);

        Task<bool> t = tcs.Task;
        t.ContinueWith(_ => rwh.Unregister(null));

        return t;
    }
}