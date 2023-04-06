using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Services;
using Base10.SparkplugB.Interfaces;
using MQTTnet.Client;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class ExposedSparkplugMqttService : SparkplugMqttService
	{

		public ExposedSparkplugMqttService() : this("", 0, false, "", "", "", "", null)
		{
		}

		public ExposedSparkplugMqttService(IMqttClient client) : this("",0, false, "", "", "", "", client)
		{
		}

		public ExposedSparkplugMqttService(string hostname, int port, bool useTls, string clientId, string username, string password, string group, IMqttClient? client) : base(hostname, port, useTls, clientId, username, password, group, client)
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

		internal new Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
		{
			return base.OnMessageReceived(args);
		}
	}
}
