using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Interfaces;

namespace Base10.SparkplugB.Core.Data
{
	public class BasicDeviceMetric : BasicNodeMetric, IDeviceMetric
	{
		public string DeviceName => throw new NotImplementedException();
	}
}
