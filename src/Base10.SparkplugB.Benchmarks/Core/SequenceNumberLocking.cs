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
	|     LockingBenchmark | 18.768 ns | 0.1029 ns | 0.0963 ns |
	| InterlockedBenchmark |  9.867 ns | 0.1573 ns | 0.1471 ns |  <--- winner!
	*/

	public class SequenceNumberLocking
	{
		private int _sequenceLocked;
		private object _sequenceLock = new object();
		private long _sequenceInterlocked;

		[GlobalSetup]
		public void Setup()
		{
			_sequenceLocked = 0;
			_sequenceLock = new object();
			_sequenceInterlocked = 0;
		}

		[Benchmark]
		public int LockingBenchmark()
		{
			lock (_sequenceLock)
			{
				_sequenceLocked++;
				if (_sequenceLocked > 255) _sequenceLocked = 0;
			}
			return _sequenceLocked;
		}

		[Benchmark]
		public int InterlockedBenchmark()
		{
			Interlocked.Increment(ref _sequenceInterlocked);
			return (int)(_sequenceInterlocked % 256);
		}
	}
}
