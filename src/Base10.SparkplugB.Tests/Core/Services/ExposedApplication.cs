using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Services;
using Base10.SparkplugB.Interfaces;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class ExposedApplication : SparkplugApplication
	{

		public ExposedApplication() : base("", "", "", "", "", new InMemoryMetricStorage())
		{
		}

		public ExposedApplication(string mqttServerUri, string clientId, string username, string password, string group, IMetricStorage metricStorage) : base(mqttServerUri, clientId, username, password, group, metricStorage)
		{
		}

		public new int NextSequence()
		{
			return base.NextSequence();
		}
	}
}
