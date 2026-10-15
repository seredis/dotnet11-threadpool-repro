// A Redis-free reproduction of the .NET 11 thread pool regression seen with StackExchange.Redis.
//
//   repro serve  --port 7000                    the echo server (blocking threads; no thread pool)
//   repro client --port 7000 --callers 64 ...   the measured client; prints one JSON line
//
// The client has the shape of a multiplexed database client: many callers share ONE socket. A caller
// writes a fixed-size request and awaits a TaskCompletionSource; one read loop receives replies and
// completes the callers in order. Continuations run asynchronously, so every reply is a hand-off
// through the thread pool - which is what the .NET 11 thread pool changes affect.
//
// Shapes (all optional):
//   --work-us N   each caller burns N us of CPU after every operation (application work)
//   --hit P       a fraction P of operations completes synchronously without I/O (a local cache hit)

using System.Net;
using System.Net.Sockets;
using Repro;

var opts = Options.Parse(args);
switch (opts.Mode)
{
    case "serve":
        EchoServer.Run(opts.Port, opts.RequestBytes, opts.ReplyBytes);
        return 0;
    case "client":
        Driver.ApplyMinThreads(opts);
        var muxes = new Mux[opts.Connections];
        for (int i = 0; i < muxes.Length; i++)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            await socket.ConnectAsync(opts.Host, opts.Port);
            muxes[i] = new Mux(socket, opts.RequestBytes, opts.ReplyBytes);
        }
        var rngs = new Random[opts.Callers];
        for (int i = 0; i < rngs.Length; i++) rngs[i] = new Random(i * 7919 + 1);
        double hit = opts.Hit;
        await Driver.RunAsync(opts, caller =>
            hit > 0 && rngs[caller].NextDouble() < hit
                ? ValueTask.CompletedTask // a local hit: completes synchronously, no I/O, no thread pool hop
                : new ValueTask(muxes[caller % muxes.Length].RequestAsync()),
            new() { ["hit"] = opts.Hit, ["request_bytes"] = opts.RequestBytes, ["reply_bytes"] = opts.ReplyBytes });
        return 0;
    default:
        Console.Error.WriteLine("usage: repro serve|client [options]; see the top of Program.cs");
        return 2;
}

namespace Repro
{
    /// <summary>
    /// Replies to every complete request with a fixed-size reply. One blocking thread per connection,
    /// and as many replies per send as there were complete requests in the read, so the server never
    /// touches the thread pool and behaves the same whichever runtime it is on.
    /// </summary>
    internal static class EchoServer
    {
        public static void Run(int port, int requestBytes, int replyBytes)
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(512);
            Console.WriteLine($"listening on {port}");
            while (true)
            {
                var socket = listener.Accept();
                socket.NoDelay = true;
                new Thread(() => Serve(socket, requestBytes, replyBytes)) { IsBackground = true }.Start();
            }
        }

        private static void Serve(Socket socket, int requestBytes, int replyBytes)
        {
            var input = new byte[64 * 1024];
            var output = new byte[replyBytes * 256];
            for (int i = 0; i < output.Length; i++) output[i] = (byte)('a' + i % 26);
            int carry = 0;
            try
            {
                while (true)
                {
                    int read = socket.Receive(input);
                    if (read <= 0) break;
                    carry += read;
                    int requests = carry / requestBytes;
                    carry %= requestBytes;
                    while (requests > 0)
                    {
                        int batch = Math.Min(requests, 256);
                        int bytes = batch * replyBytes, sent = 0;
                        while (sent < bytes) sent += socket.Send(output, sent, bytes - sent, SocketFlags.None);
                        requests -= batch;
                    }
                }
            }
            catch (SocketException) { }
            finally { socket.Dispose(); }
        }
    }

    /// <summary>One multiplexed connection: callers write and enqueue; one read loop completes them in order.</summary>
    internal sealed class Mux
    {
        private readonly Socket _socket;
        private readonly int _requestBytes, _replyBytes;
        private readonly object _sync = new();
        private readonly Queue<TaskCompletionSource> _pending = new();
        private byte[] _write = new byte[64 * 1024], _spare = new byte[64 * 1024];
        private int _writeLength;
        private bool _flushing;

        public Mux(Socket socket, int requestBytes, int replyBytes)
        {
            _socket = socket;
            _requestBytes = requestBytes;
            _replyBytes = replyBytes;
            _ = Task.Run(ReadLoopAsync);
        }

        public Task RequestAsync()
        {
            // RunContinuationsAsynchronously: the read loop must not run caller code, so every
            // completion is a thread pool work item - as in most multiplexed clients.
            var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            bool flush = false;
            lock (_sync)
            {
                _pending.Enqueue(pending);
                if (_writeLength + _requestBytes > _write.Length) Array.Resize(ref _write, _write.Length * 2);
                _write.AsSpan(_writeLength, _requestBytes).Fill((byte)'r');
                _writeLength += _requestBytes;
                if (!_flushing) flush = _flushing = true;
            }
            if (flush) _ = FlushAsync();
            return pending.Task;
        }

        // The caller that finds no flush in progress writes everything queued, including what other
        // callers add meanwhile, then hands the role back.
        private async Task FlushAsync()
        {
            while (true)
            {
                byte[] buffer;
                int length;
                lock (_sync)
                {
                    if (_writeLength == 0) { _flushing = false; return; }
                    buffer = _write; length = _writeLength;
                    _write = _spare; _spare = buffer; _writeLength = 0;
                    if (_write.Length < buffer.Length) _write = new byte[buffer.Length];
                }
                int sent = 0;
                while (sent < length)
                    sent += await _socket.SendAsync(buffer.AsMemory(sent, length - sent), SocketFlags.None).ConfigureAwait(false);
            }
        }

        private async Task ReadLoopAsync()
        {
            var buffer = new byte[64 * 1024];
            var batch = new TaskCompletionSource[1024];
            int carry = 0;
            while (true)
            {
                int read = await _socket.ReceiveAsync(buffer.AsMemory(), SocketFlags.None).ConfigureAwait(false);
                if (read <= 0) return;
                carry += read;
                int replies = carry / _replyBytes;
                carry %= _replyBytes;
                while (replies > 0)
                {
                    int n = Math.Min(replies, batch.Length);
                    lock (_sync)
                    {
                        for (int i = 0; i < n; i++) batch[i] = _pending.Dequeue();
                    }
                    for (int i = 0; i < n; i++) { batch[i].SetResult(); batch[i] = null!; }
                    replies -= n;
                }
            }
        }
    }
}
