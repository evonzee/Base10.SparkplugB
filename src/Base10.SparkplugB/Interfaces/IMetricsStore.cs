using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Base10.SparkplugB.Protocol.Payload.Types;

namespace Base10.SparkplugB.Interfaces
{
	public interface IMetricsStore
	{
		public Task AddMetricAsync(string group, string node, string? device, Metric metric);
		public Task<bool> SetMetricAsync(string group, string node, string? device, Metric metric);
		public Task<Metric?> GetMetricAsync(string group, string node, string? device, string metricName);
		public Task<Metric?> GetMetricAsync(string group, string node, string? device, ulong alias);
		public Task ClearAsync(string group, string node, string? device);
		public Task ClearAsync(string group);
		public Task ClearAsync();
	}
}
