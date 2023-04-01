using System;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Interfaces;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Base10.SparkplugB.Core.Services
{
	public class SparkplugApplication : SparkplugMqttService
	{
		public SparkplugApplication(string mqttServerUri, string clientId, string username, string password, string group) : base(mqttServerUri, clientId, username, password, group)
		{
		}

		protected override MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder optionsBuilder)
		{
			throw new NotImplementedException();
		}

		protected override Task SendBirthSequence(IManagedMqttClient mqttClient)
		{
			throw new NotImplementedException();
		}

		protected override Task Subscribe(IManagedMqttClient mqttClient)
		{
			throw new NotImplementedException();
		}
	}
}
