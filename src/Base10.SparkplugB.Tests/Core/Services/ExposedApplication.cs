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

		public ExposedApplication() : base("", "", "", "", "")
		{
		}

		public ExposedApplication(string mqttServerUri, string clientId, string username, string password, string group) : base(mqttServerUri, clientId, username, password, group)
		{
		}

		public new int NextCommandSequence()
		{
			return base.NextCommandSequence();
		}

		public new int NextBirthSequence()
		{
			return base.NextBirthSequence();
		}
	}
}
