# MultiLatency

A simple program to check latency to multiple hosts.

## Usage

```
MultiLatency [OPTIONS] [HOSTS...]
Options:
    -help       Show this help.
    -time=n     How long to measure latency
    -timeout=n  Timeout for each ping
If no hosts are given, hosts are read from standard input until EOF.
```

For example:

```
MultiLatency 1.1.1.1 1.0.0.1 8.8.8.8 8.8.4.4 example.com
```

Output:

```
Latency Received    Total   Loss Hostname or IP
   46.8       46       46   0.0% 8.8.8.8
   47.7       43       43   0.0% 8.8.4.4
   49.3       38       38   0.0% 1.0.0.1
   53.6       42       42   0.0% 1.1.1.1
  153.3       26       26   0.0% example.com
```

## License

```
BSD Zero Clause License

Copyright (c) 2022 namazso <admin@namazso.eu>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
```