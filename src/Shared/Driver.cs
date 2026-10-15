// Shared by both apps: options, the measured caller loop, and the JSON result line.

using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Repro;

internal sealed class Options
{
    public string Mode = "";
    public string Host = "127.0.0.1";
    public int Port = 7000;
    public int Callers = 64;
    public int Connections = 1;
    public int RequestBytes = 32;
    public int ReplyBytes = 1024;
    public double WorkUs;
    public double Hit;
    public bool Cache;
    public int CacheMb = 32;
    public int Keyspace = 1000;
    public double ZipfS;
    public int MinThreads;
    public double WarmupSeconds = 3;
    public double DurationSeconds = 10;
    public string Label = "";

    public static Options Parse(string[] args)
    {
        var o = new Options { Mode = args.Length > 0 ? args[0] : "" };
        for (int i = 1; i < args.Length; i++)
        {
            string Next() => args[++i];
            switch (args[i])
            {
                case "--host": o.Host = Next(); break;
                case "--port": o.Port = int.Parse(Next()); break;
                case "--callers": o.Callers = int.Parse(Next()); break;
                case "--connections": o.Connections = int.Parse(Next()); break;
                case "--request-bytes": o.RequestBytes = int.Parse(Next()); break;
                case "--reply-bytes": o.ReplyBytes = int.Parse(Next()); break;
                case "--work-us": o.WorkUs = double.Parse(Next()); break;
                case "--hit": o.Hit = double.Parse(Next()); break;
                case "--cache": o.Cache = true; break;
                case "--cache-mb": o.CacheMb = int.Parse(Next()); break;
                case "--keyspace": o.Keyspace = int.Parse(Next()); break;
                case "--zipf": o.ZipfS = double.Parse(Next()); break;
                case "--min-threads": o.MinThreads = int.Parse(Next()); break;
                case "--warmup": o.WarmupSeconds = double.Parse(Next()); break;
                case "--duration": o.DurationSeconds = double.Parse(Next()); break;
                case "--label": o.Label = Next(); break;
                default: throw new ArgumentException($"unknown option {args[i]}");
            }
        }
        return o;
    }
}

/// <summary>
/// Runs <see cref="Options.Callers"/> concurrent callers, each looping: one operation, then
/// <see cref="Options.WorkUs"/> of CPU. Measures a window after a warmup and prints one JSON line.
/// </summary>
internal static class Driver
{
    private static volatile bool s_stop;

    public static void ApplyMinThreads(Options o)
    {
        if (o.MinThreads <= 0) return; // leave the runtime's default (processor count) alone
        ThreadPool.GetMinThreads(out _, out int io);
        ThreadPool.SetMinThreads(o.MinThreads, Math.Max(io, o.MinThreads));
    }

    public static async Task RunAsync(Options o, Func<int, ValueTask> operation, Dictionary<string, object?> extra)
    {
        var counters = new Counter[o.Callers];
        var histograms = new Histogram[o.Callers];
        for (int i = 0; i < o.Callers; i++) histograms[i] = new Histogram();
        long workTicks = (long)(o.WorkUs * Stopwatch.Frequency / 1_000_000);
        var callers = new Task[o.Callers];
        for (int i = 0; i < o.Callers; i++)
        {
            int id = i;
            callers[i] = Task.Run(() => CallerAsync(id, operation, workTicks, counters, histograms[id]));
        }

        await Task.Delay(TimeSpan.FromSeconds(o.WarmupSeconds));
        long ops0 = Sum(counters);
        foreach (var h in histograms) h.Reset();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var user0 = process.UserProcessorTime; var total0 = process.TotalProcessorTime;
        int gc0 = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        long completed0 = ThreadPool.CompletedWorkItemCount;
        long t0 = Stopwatch.GetTimestamp();
        await Task.Delay(TimeSpan.FromSeconds(o.DurationSeconds));
        long ops1 = Sum(counters);
        long t1 = Stopwatch.GetTimestamp();
        long completed1 = ThreadPool.CompletedWorkItemCount;
        process.Refresh();
        var user1 = process.UserProcessorTime; var total1 = process.TotalProcessorTime;
        int gc1 = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        int threads = ThreadPool.ThreadCount;
        s_stop = true;

        var merged = new Histogram();
        foreach (var h in histograms) merged.Add(h);
        double seconds = (t1 - t0) / (double)Stopwatch.Frequency;
        long ops = ops1 - ops0;
        ThreadPool.GetMinThreads(out int minWorker, out _);
        var result = new Dictionary<string, object?>
        {
            ["label"] = o.Label,
            ["mode"] = o.Mode,
            ["runtime"] = RuntimeInformation.FrameworkDescription,
            ["runtime_build"] = typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            ["os"] = RuntimeInformation.OSDescription,
            ["processor_count"] = Environment.ProcessorCount,
            ["min_worker_threads"] = minWorker,
            ["env"] = ThreadPoolEnv(),
            ["async_compilation"] = AsyncCompilation.Describe(),
            ["callers"] = o.Callers,
            ["connections"] = o.Connections,
            ["work_us"] = o.WorkUs,
            ["seconds"] = seconds,
            ["ops"] = ops,
            ["ops_per_sec"] = ops / seconds,
            ["cpu_us_per_op"] = (total1 - total0).TotalMicroseconds / Math.Max(1, ops),
            ["cpu_user_us_per_op"] = (user1 - user0).TotalMicroseconds / Math.Max(1, ops),
            ["cpu_cores_busy"] = (total1 - total0).TotalSeconds / seconds,
            ["threadpool_items_per_op"] = (completed1 - completed0) / (double)Math.Max(1, ops),
            ["threadpool_threads"] = threads,
            ["op_latency_us"] = new Dictionary<string, double>
            {
                ["p50"] = merged.PercentileUs(0.50),
                ["p90"] = merged.PercentileUs(0.90),
                ["p99"] = merged.PercentileUs(0.99),
                ["p999"] = merged.PercentileUs(0.999),
            },
            ["gc_collections"] = gc1 - gc0,
        };
        foreach (var (k, v) in extra) result[k] = v;
        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    private static async Task CallerAsync(int id, Func<int, ValueTask> operation, long workTicks, Counter[] counters, Histogram histogram)
    {
        long ops = 0;
        while (!s_stop)
        {
            long start = Stopwatch.GetTimestamp();
            await operation(id).ConfigureAwait(false);
            histogram.Record(Stopwatch.GetTimestamp() - start);
            if (workTicks > 0) Spin(workTicks);
            // published every op so the measurement window can be cut anywhere
            Volatile.Write(ref counters[id].Value, ++ops);
        }
    }

    /// <summary>Application work: burns the given Stopwatch ticks of CPU on the calling thread.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Spin(long ticks)
    {
        long until = Stopwatch.GetTimestamp() + ticks;
        while (Stopwatch.GetTimestamp() < until) { }
    }

    private static long Sum(Counter[] counters)
    {
        long sum = 0;
        for (int i = 0; i < counters.Length; i++) sum += Volatile.Read(ref counters[i].Value);
        return sum;
    }

    // Every DOTNET_* variable that could change the runtime's behaviour, so a result says what it ran under.
    private static SortedDictionary<string, string> ThreadPoolEnv()
    {
        string[] imageDefaults = ["DOTNET_VERSION", "DOTNET_SDK_VERSION", "DOTNET_RUNNING_IN_CONTAINER", "DOTNET_NOLOGO",
            "DOTNET_USE_POLLING_FILE_WATCHER", "DOTNET_GENERATE_ASPNET_CERTIFICATE", "DOTNET_ROOT"];
        var env = new SortedDictionary<string, string>();
        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            var key = (string)e.Key;
            if (key.StartsWith("DOTNET_", StringComparison.Ordinal) && Array.IndexOf(imageDefaults, key) < 0)
                env[key] = (string)e.Value!;
        }
        return env;
    }
}

// one cache line per caller, so the counters do not contend
[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct Counter
{
    [FieldOffset(64)] public long Value;
}

/// <summary>Log-linear latency histogram over Stopwatch ticks (16 sub-buckets per power of two).</summary>
internal sealed class Histogram
{
    private const int Sub = 16;
    private readonly long[] _counts = new long[64 * Sub];

    public void Record(long ticks)
    {
        if (ticks < 1) ticks = 1;
        int log = 63 - BitOperations.LeadingZeroCount((ulong)ticks);
        int sub = log < 4 ? (int)(ticks & (Sub - 1)) : (int)((ticks >> (log - 4)) & (Sub - 1));
        _counts[log * Sub + sub]++;
    }

    public void Reset() => Array.Clear(_counts);

    public void Add(Histogram other)
    {
        for (int i = 0; i < _counts.Length; i++) _counts[i] += other._counts[i];
    }

    public double PercentileUs(double q)
    {
        long total = 0;
        foreach (var c in _counts) total += c;
        if (total == 0) return 0;
        long target = (long)Math.Ceiling(q * total), seen = 0;
        for (int i = 0; i < _counts.Length; i++)
        {
            seen += _counts[i];
            if (seen >= target)
            {
                int log = i / Sub, sub = i % Sub;
                double ticks = log < 4 ? sub : (double)((1L << log) | ((long)sub << (log - 4)));
                return ticks * 1_000_000 / Stopwatch.Frequency;
            }
        }
        return 0;
    }
}

/// <summary>
/// How async methods were compiled, read back from metadata: an [AsyncStateMachine] attribute marks a
/// compiler state machine; MethodImplAttributes 0x2000 marks runtime async (.NET 11). Reported for the
/// app, any libraries named, and a sample of the runtime's own assemblies.
/// </summary>
internal static class AsyncCompilation
{
    private const int MethodImplAsync = 0x2000;

    public static Assembly[] Libraries = [];

    public static SortedDictionary<string, string> Describe()
    {
        var result = new SortedDictionary<string, string>();
        var assemblies = new List<Assembly> { Assembly.GetEntryAssembly()!, typeof(object).Assembly, typeof(Socket).Assembly };
        assemblies.AddRange(Libraries);
        foreach (var assembly in assemblies)
        {
            var (stateMachines, native) = Count(assembly);
            result[assembly.GetName().Name!] = $"{native} runtime-async / {stateMachines} state-machine";
        }
        return result;
    }

    private static (int StateMachines, int RuntimeAsync) Count(Assembly assembly)
    {
        using var pe = new System.Reflection.PortableExecutable.PEReader(File.OpenRead(assembly.Location));
        var md = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pe);
        int stateMachines = 0, native = 0;
        foreach (var handle in md.MethodDefinitions)
        {
            var method = md.GetMethodDefinition(handle);
            if (((int)method.ImplAttributes & MethodImplAsync) != 0) native++;
            foreach (var attributeHandle in method.GetCustomAttributes())
            {
                var attribute = md.GetCustomAttribute(attributeHandle);
                if (attribute.Constructor.Kind != System.Reflection.Metadata.HandleKind.MemberReference) continue;
                var ctor = md.GetMemberReference((System.Reflection.Metadata.MemberReferenceHandle)attribute.Constructor);
                if (ctor.Parent.Kind == System.Reflection.Metadata.HandleKind.TypeReference
                    && md.StringComparer.Equals(
                        md.GetTypeReference((System.Reflection.Metadata.TypeReferenceHandle)ctor.Parent).Name,
                        "AsyncStateMachineAttribute"))
                {
                    stateMachines++;
                }
            }
        }
        return (stateMachines, native);
    }
}
