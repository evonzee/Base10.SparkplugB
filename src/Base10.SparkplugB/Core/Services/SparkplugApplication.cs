using System;
using System.Text.Json;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Enums;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace Base10.SparkplugB.Core.Services
{
	public class SparkplugApplication : SparkplugMqttService
	{
		private long _lastBirthTimestamp = 0;

		public SparkplugApplication(SparkplugServiceOptions options, IMqttClient? mqttClient = null, ILogger? logger = null) : base(options, mqttClient, logger)
		{
			this.Connected += OnConnected;
		}

		private async Task OnConnected(EventArgs e)
		{
			// subscribe to appropriate topics per configuration
			// subscribe happens before birth, per [tck-id-host-topic-phid-birth-required]
			await SubscribeInitial(_mqttClient);

			// send birth message
			await SendBirthSequence(_mqttClient); // apps must satisfy [tck-id-components-ph-state]
		}

		protected override MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder builder)
		{
			_lastBirthTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var willPayload = new { online = false, timestamp = _lastBirthTimestamp }; // [tck-id-host-topic-phid-death-payload-connect]
			return builder
				.WithWillContentType("application/json")
				.WithWillDelayInterval(0)
				.WithWillTopic(new SparkplugTopic(CommandType.STATE, _nodeName).ToMqttTopic()) // [tck-id-host-topic-phid-death-topic]
				.WithWillPayload(JsonSerializer.Serialize(willPayload)) //[tck-id-host-topic-phid-death-payload]
				.WithWillRetain(true) // [tck-id-host-topic-phid-death-retain]
				.WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // [tck-id-host-topic-phid-death-qos]
				;
		}

		private async Task SubscribeInitial(IMqttClient mqttClient)
		{
			await mqttClient.SubscribeAsync(CommandType.STATE.GetSubscriptionPattern(), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			await mqttClient.SubscribeAsync(CommandType.NBIRTH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			await mqttClient.SubscribeAsync(CommandType.NDEATH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			await mqttClient.SubscribeAsync(CommandType.NDATA.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			await mqttClient.SubscribeAsync(CommandType.DBIRTH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			await mqttClient.SubscribeAsync(CommandType.DDEATH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			await mqttClient.SubscribeAsync(CommandType.DDATA.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
		}

		private async Task SendBirthSequence(IMqttClient mqttClient)
		{
			var willPayload = new { online = true, timestamp = _lastBirthTimestamp };
			await mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(CommandType.STATE, _nodeName).ToMqttTopic())
				.WithPayload(JsonSerializer.Serialize(willPayload))
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
				.WithRetainFlag(true)
				.Build());
		}



	}
}
