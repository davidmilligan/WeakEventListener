``` ini

BenchmarkDotNet=v0.11.5, OS=macOS High Sierra 10.13 (17A405) [Darwin 17.0.0]
Intel Core i5-3427U CPU 1.80GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview6-012264
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT


```
|                  Method |         Mean |       Error |      StdDev |  Ratio | RatioSD |
|------------------------ |-------------:|------------:|------------:|-------:|--------:|
|            RegisterWeak |  2,030.73 ns |  39.6547 ns |  55.5904 ns |  44.83 |    1.61 |
|      RegisterWeakCustom | 14,834.23 ns | 157.6389 ns | 123.0741 ns | 327.20 |    5.01 |
|       RegisterWeakTyped |    913.54 ns |  17.9917 ns |  21.4178 ns |  20.15 |    0.53 |
|    RegisterWeakProperty |    952.44 ns |  19.9278 ns |  37.4293 ns |  21.43 |    1.07 |
| RegisterWeakTypedCustom | 11,739.18 ns | 122.4618 ns | 108.5592 ns | 258.88 |    4.59 |
|          RegisterNormal |     45.35 ns |   0.6085 ns |   0.5394 ns |   1.00 |    0.00 |
