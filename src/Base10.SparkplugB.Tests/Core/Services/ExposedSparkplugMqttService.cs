using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Services;
using Base10.SparkplugB.Interfaces;
using MQTTnet.Client;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class ExposedSparkplugMqttService : SparkplugMqttService
	{

		public ExposedSparkplugMqttService() : this(null)
		{
		}

		public ExposedSparkplugMqttService(IMqttClient? client) : this(new SparkplugServiceOptions(), client)
		{
		}

		public ExposedSparkplugMqttService(SparkplugServiceOptions options, IMqttClient? client) : base(options, client)
		{
		}

		public new ulong NextCommandSequence()
		{
			return base.NextCommandSequence();
		}

		public new ulong NextBirthSequence()
		{
			return base.NextBirthSequence();
		}

		internal new Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
		{
			return base.OnMessageReceived(args);
		}
	}
}
