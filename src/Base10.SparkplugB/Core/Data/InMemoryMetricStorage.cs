using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Interfaces;
using Base10.SparkplugB.Protocol;

namespace Base10.SparkplugB.Core.Data
{
	public class InMemoryMetricStorage : IMetricStorage
	{
		public Task<IDeviceMetric> AddDeviceMetric(string deviceName, string name, DataType type, object value, bool hasAlias = true)
		{
			throw new NotImplementedException();
		}

		public Task<IDeviceMetric> AddDeviceMetric(IDeviceMetric metric)
		{
			throw new NotImplementedException();
		}

		public Task<IMetric> AddNodeMetric(string name, DataType type, object value, bool hasAlias = true)
		{
			throw new NotImplementedException();
		}

		public Task<IMetric> AddNodeMetric(IMetric metric)
		{
			throw new NotImplementedException();
		}

		public Task<int> GetAlias(string name)
		{
			throw new NotImplementedException();
		}

		public Task<int> GetAlias(string deviceName, string name)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<IMetric>> GetAll()
		{
			throw new NotImplementedException();
		}

		public Task<IMetric> GetByAlias(int alias)
		{
			throw new NotImplementedException();
		}
	}
}
