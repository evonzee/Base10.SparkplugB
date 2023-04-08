using System;
using System.Text.Json;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace Base10.SparkplugB.Core.Services
{
	public class SparkplugApplication : SparkplugMqttService
	{
		public SparkplugApplication(SparkplugServiceOptions options, IMqttClient? mqttClient = null, ILogger? logger = null) : base(options, mqttClient, logger)
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

		protected override MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder builder)
		{
			var willPayload = new { online = false, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; // [tck-id-host-topic-phid-death-payload-connect]
			return builder
				.WithWillContentType("application/json")
				.WithWillDelayInterval(0)
				.WithWillPayload(JsonSerializer.Serialize(willPayload)) //[tck-id-host-topic-phid-death-payload]
				.WithWillRetain(true) // [tck-id-host-topic-phid-death-retain]
				.WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // [tck-id-host-topic-phid-death-qos]
				;
		}


	}
}
