using BenchmarkDotNet.Running;

using GTMH.Benchmarking;

#if DEBUG
Console.WriteLine("***************************************");
Console.WriteLine("***************************************");
Console.WriteLine("** Run Benchmarking in Release Mode **");
Console.WriteLine("** Run Benchmarking in Release Mode **");
Console.WriteLine("***************************************");
Console.WriteLine("***************************************");
#endif

BenchmarkRunner.Run<S11nBenchmarks>();
