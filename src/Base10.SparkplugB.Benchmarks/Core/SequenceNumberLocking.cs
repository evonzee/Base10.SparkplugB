using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Base10.SparkplugB.Benchmarks.Core
{
	/* Results:
	BenchmarkDotNet=v0.13.5, OS=arch
	Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
	.NET SDK=7.0.103
	[Host]     : .NET 7.0.3 (7.0.323.12801), X64 RyuJIT AVX
	DefaultJob : .NET 7.0.3 (7.0.323.12801), X64 RyuJIT AVX

	|               Method |      Mean |     Error |    StdDev |
	|--------------------- |----------:|----------:|----------:|
	|     LockingBenchmark | 18.516 ns | 0.1019 ns | 0.0954 ns |
	| InterlockedBenchmark |  9.789 ns | 0.0632 ns | 0.0592 ns |  <--- winner!
*/

	public class SequenceNumberLocking
	{
		private int _sequenceLocked;
		private object _sequenceLock;
		private int _sequenceInterlocked;

		[GlobalSetup]
		public void Setup()
		{
			_sequenceLocked = 0;
			_sequenceLock = new object();
			_sequenceInterlocked = 0;
		}

		[Benchmark]
		public void LockingBenchmark()
		{
			lock (_sequenceLock)
			{
				_sequenceLocked++;
				if (_sequenceLocked > 255) _sequenceLocked = 0;
			}
		}

		[Benchmark]
		public int InterlockedBenchmark()
		{
			Interlocked.Increment(ref _sequenceInterlocked);
			return _sequenceInterlocked % 256;
		}
	}
}
