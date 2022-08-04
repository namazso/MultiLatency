//  Copyright (c) 2022 namazso <admin@namazso.eu>
//  
//  Permission to use, copy, modify, and/or distribute this software for any
//  purpose with or without fee is hereby granted.
//  
//  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
//  REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
//  AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
//  INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
//  LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
//  OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
//  PERFORMANCE OF THIS SOFTWARE.

using System.Net.NetworkInformation;

var hosts = new HashSet<string>();
var time = 30;
var timeout = 5000;
if (args.Length > 0)
{
    foreach (var arg in args)
    {
        var trimmed = arg.Trim().ToLower();
        if (trimmed.Length == 0)
            continue;
        if (trimmed[0] == '-')
        {
            var split = trimmed.TrimStart('-').Split('=');
            switch (split[0])
            {
                case "time":
                    time = int.Parse(split[1]);
                    break;
                case "timeout":
                    timeout = int.Parse(split[1]);
                    break;
                case "help":
                    Console.Out.WriteLine(@"MultiLatency - Latency measurement tool
Usage: MultiLatency [OPTIONS] [HOSTS...]
Options:
    -help       Show this help.
    -time=n     How long to measure latency
    -timeout=n  Timeout for each ping
If no hosts are given, hosts are read from standard input until EOF.
");
                    return 0;
            }
        }
        else
        {
            hosts.Add(trimmed);
        }
    }
}


if (hosts.Count == 0)
    while (Console.ReadLine() is { } line)
        hosts.Add(line.Trim());

var hostInfos = hosts.Select(host => new HostInfo(host, timeout)).ToArray();

foreach (var hostInfo in hostInfos)
    hostInfo.DoPing();

Thread.Sleep(TimeSpan.FromSeconds(time));

foreach (var hostInfo in hostInfos)
    hostInfo.SendStop();

foreach (var hostInfo in hostInfos)
    hostInfo.WaitStop();

Console.Out.WriteLine($"{"Latency",7} {"Received",8} {"Total",8} {"Loss",6} Hostname or IP");
foreach (var info in hostInfos.OrderBy(hostInfo => hostInfo.GetAverage()))
{
    if (info.GetReceived() != 0)
    {
        Console.Out.WriteLine("{0,7:F1} {1,8} {2,8} {3,6:P1} {4}",
            info.GetAverage(),
            info.GetReceived(),
            info.GetTotal(),
            info.GetLoss(),
            info.HostnameOrIp
        );
    }
    else
    {
        Console.Out.WriteLine($"ERROR                            {info.HostnameOrIp}");
    }
}

return 0;

class HostInfo
{
    private long _stop;
    public readonly string HostnameOrIp;
    private readonly SortedSet<long> _millis = new();
    private int _lost;
    private readonly int _timeout;
    private readonly Ping _ping = new();

    public void SendStop()
    {
        Interlocked.Increment(ref _stop);
        _ping.SendAsyncCancel();
    }

    public void WaitStop()
    {
        while (Interlocked.Read(ref _stop) != 2)
        {
        }
    }

    public int GetReceived() => _millis.Count;
    public int GetTotal() => _millis.Count + _lost;
    public int GetLost() => _lost;
    public double GetAverage() => GetReceived() != 0 ? _millis.Average() : 9999;
    public double GetLoss() => GetTotal() != 0 ? (double)_lost / GetTotal() : 1.0;

    public HostInfo(string hostnameOrIp, int timeout)
    {
        HostnameOrIp = hostnameOrIp;
        _timeout = timeout;
        _ping.PingCompleted += (_, args) => Pinged(args);
    }

    public void DoPing()
    {
        _ping.SendAsync(HostnameOrIp, _timeout, null);
    }

    private void Pinged(PingCompletedEventArgs args)
    {
        if (Interlocked.Read(ref _stop) != 0)
        {
            Interlocked.Increment(ref _stop);
            return;
        }

        if (args.Reply != null)
        {
            _millis.Add(args.Reply.RoundtripTime);
        }
        else
        {
            _lost += 1;
        }
        
        DoPing();
    }
};