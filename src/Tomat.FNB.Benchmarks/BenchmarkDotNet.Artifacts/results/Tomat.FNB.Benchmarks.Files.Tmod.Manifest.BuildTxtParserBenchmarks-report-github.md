```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 5700G with Radeon Graphics 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method    | Mean     | Error    | StdDev    | Median   | Ratio | RatioSD | Gen0    | Gen1    | Allocated | Alloc Ratio |
|---------- |---------:|---------:|----------:|---------:|------:|--------:|--------:|--------:|----------:|------------:|
| TmlStyle  | 64.84 μs | 3.327 μs |  9.757 μs | 64.83 μs |  1.02 |    0.22 | 37.2314 | 11.5967 | 304.64 KB |        1.00 |
| NewParser | 67.23 μs | 4.419 μs | 13.028 μs | 60.46 μs |  1.06 |    0.26 |  3.9063 |       - |  32.24 KB |        0.11 |
