// The same measured loop as ../Repro, with StackExchange.Redis (v3 or v4) against a real Redis:
// every operation is a GET of a 1 KiB value through one shared ConnectionMultiplexer.
//
//   redisrepro client --port 6379 --callers 64 [--work-us 20] [--keyspace 1000]
//   redisrepro client --port 6379 --callers 64 --cache --keyspace 200000 --zipf 0.99     (v4 only)

using System.Diagnostics;
using Repro;
using StackExchange.Redis;
#if SER_V4
using StackExchange.Redis.Caching;
#endif

var o = Options.Parse(args);
if (o.Mode != "client")
{
    Console.Error.WriteLine("usage: redisrepro client [options]; see the top of Program.cs");
    return 2;
}
Driver.ApplyMinThreads(o);

var config = new ConfigurationOptions { AbortOnConnectFail = true, ConnectTimeout = 10_000, SyncTimeout = 30_000, AsyncTimeout = 30_000 };
config.EndPoints.Add(o.Host, o.Port);
#if SER_V4
if (o.Cache)
{
    config.ClientCache = new CacheOptions
    {
        Enabled = true,
        MaxBytes = (long)o.CacheMb * 1024 * 1024,
        DefaultPolicy = new CachePolicy { TimeToLive = TimeSpan.FromHours(1) },
    };
}
#else
if (o.Cache) throw new NotSupportedException("client-side caching is a v4 feature");
#endif

using var mux = await ConnectionMultiplexer.ConnectAsync(config);
var db = mux.GetDatabase();
#if SER_V4
var strings = mux.GetDatabaseContext().Strings;
#endif

// seed every key with a 1 KiB value, so every GET returns one
var keys = new RedisKey[o.Keyspace];
var payload = new byte[1024];
Random.Shared.NextBytes(payload);
for (int i = 0; i < keys.Length; i++) keys[i] = $"tp:{o.Keyspace}:{i}";
for (int start = 0; start < keys.Length; start += 1000)
{
    var batch = new List<Task>();
    for (int i = start; i < Math.Min(keys.Length, start + 1000); i++) batch.Add(db.StringSetAsync(keys[i], payload));
    await Task.WhenAll(batch);
}

// The key each caller reads next: uniform walks the keyspace in order; Zipf walks a fixed,
// seeded sequence of 4M draws, each caller starting 1/callers of the way along it.
int[] sequence;
if (o.ZipfS > 0)
{
    var cumulative = new double[o.Keyspace];
    double sum = 0;
    for (int k = 0; k < o.Keyspace; k++) cumulative[k] = sum += 1.0 / Math.Pow(k + 1, o.ZipfS);
    var rng = new Random(20261010);
    sequence = new int[1 << 22];
    for (int i = 0; i < sequence.Length; i++)
    {
        int at = Array.BinarySearch(cumulative, rng.NextDouble() * sum);
        sequence[i] = at >= 0 ? at : Math.Min(~at, o.Keyspace - 1);
    }
}
else
{
    sequence = new int[o.Keyspace];
    for (int i = 0; i < sequence.Length; i++) sequence[i] = i;
}
var position = new int[o.Callers];
for (int c = 0; c < o.Callers; c++) position[c] = (int)((long)c * sequence.Length / o.Callers);

AsyncCompilation.Libraries = [typeof(ConnectionMultiplexer).Assembly, .. AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "RESPite")];
string version = typeof(ConnectionMultiplexer).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
    .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ?? "?";

await Driver.RunAsync(o, GetAsync, new()
{
    ["library"] = $"StackExchange.Redis {version}",
    ["library_ref"] = Environment.GetEnvironmentVariable("SER_REF"),
    ["cache"] = o.Cache,
    ["cache_mb"] = o.Cache ? o.CacheMb : null,
    ["keyspace"] = o.Keyspace,
    ["zipf_s"] = o.ZipfS,
});
return 0;

async ValueTask GetAsync(int caller)
{
    int at = position[caller];
    position[caller] = at + 1 == sequence.Length ? 0 : at + 1;
#if SER_V4
    var value = await strings.GetAsync(keys[sequence[at]]).ConfigureAwait(false);
#else
    var value = await db.StringGetAsync(keys[sequence[at]]).ConfigureAwait(false);
#endif
    if (value.IsNull) throw new InvalidOperationException("missing key");
}
