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
