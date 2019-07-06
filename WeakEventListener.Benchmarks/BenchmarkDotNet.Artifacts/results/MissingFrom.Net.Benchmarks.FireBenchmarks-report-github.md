``` ini

BenchmarkDotNet=v0.11.5, OS=macOS High Sierra 10.13 (17A405) [Darwin 17.0.0]
Intel Core i5-3427U CPU 1.80GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview6-012264
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT


```
|     Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
|----------- |---------:|----------:|----------:|------:|--------:|
|   FireWeak | 37.67 ns | 0.6102 ns | 0.5708 ns |  2.37 |    0.04 |
| FireNormal | 15.94 ns | 0.2783 ns | 0.2467 ns |  1.00 |    0.00 |
