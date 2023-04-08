using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace Base10.SparkplugB.Core.Services
{
	public class SparkplugApplication : SparkplugMqttService
	{
		public SparkplugApplication(string hostname, int port, bool useTls, string clientId, string username, string password, string group, IMqttClient? mqttClient = null, ILogger? logger = null) : base(hostname, port, useTls, clientId, username, password, group, mqttClient, logger)
		{
			this.Connected += OnConnected;
		}

		private async Task OnConnected(EventArgs e)
		{
			// subscribe to appropriate topics per configuration
			// subscribe happens before birth, per [tck-id-host-topic-phid-birth-required]
			// await Subscribe(_mqttClient);

			// send birth message
			// await SendBirthSequence(_mqttClient); // apps must satisfy [tck-id-components-ph-state]
		}

		protected override MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder optionsBuilder)
		{
			throw new NotImplementedException();
		}


	}
}
