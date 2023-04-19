using System;
using System.Text.Json;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Enums;
using Base10.SparkplugB.Protocol;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace Base10.SparkplugB.Core.Services
{
	public class SparkplugApplication : SparkplugMqttService
	{
		private long _connectTimestamp = 0;
		private readonly SparkplugApplicationOptions _options;

		public SparkplugApplication(SparkplugApplicationOptions options, IMqttClient? mqttClient = null, ILogger? logger = null) : base(options, mqttClient, logger)
		{
			_options = options;
			this.ConnectedAsync += OnConnectedAsync;
			this.BeforeDisconnectAsync += OnBeforeDisconnectAsync;
		}

		private async Task OnConnectedAsync(EventArgs e)
		{
			try
			{
				// subscribe to appropriate topics per configuration
				// subscribe happens before birth, per [tck-id-host-topic-phid-birth-required]
				await SubscribeInitialAsync(_mqttClient);

				// send birth message
				await SendBirthSequenceAsync(_mqttClient); // apps must satisfy [tck-id-components-ph-state]
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to startup application!  Behavior may be somewhat strange.");
				throw;
			}
		}

		protected async Task OnBeforeDisconnectAsync(EventArgs e)
		{
			await SendDeathSequenceAsync(_mqttClient);
		}

		protected override MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder builder)
		{
			_connectTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var willPayload = new { online = false, timestamp = _connectTimestamp }; // [tck-id-host-topic-phid-death-payload-connect]
			return builder
				.WithWillContentType("application/json")
				.WithWillDelayInterval(0)
				.WithWillTopic(new SparkplugTopic(SparkplugMessageType.STATE, _nodeName).ToMqttTopic()) // [tck-id-host-topic-phid-death-topic]
				.WithWillPayload(JsonSerializer.Serialize(willPayload)) //[tck-id-host-topic-phid-death-payload]
				.WithWillRetain(true) // [tck-id-host-topic-phid-death-retain]
				.WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // [tck-id-host-topic-phid-death-qos]
				;
		}

		private async Task SubscribeInitialAsync(IMqttClient mqttClient)
		{
			var optionsBuilder = new MqttClientSubscribeOptionsBuilder()
				.WithTopicFilter(SparkplugMessageType.STATE.GetSubscriptionPattern(), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			if (_options.Promiscuous)
			{
				optionsBuilder = optionsBuilder.WithTopicFilter("spBv1.0/#");
			}
			else
			{
				optionsBuilder = optionsBuilder.WithTopicFilter(SparkplugMessageType.NBIRTH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(SparkplugMessageType.NDEATH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(SparkplugMessageType.NDATA.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(SparkplugMessageType.DBIRTH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(SparkplugMessageType.DDEATH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(SparkplugMessageType.DDATA.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			}

			var options = optionsBuilder.Build();
			await mqttClient.SubscribeAsync(options);
		}

		private async Task SendBirthSequenceAsync(IMqttClient mqttClient)
		{
			await SendStatusAsync(mqttClient, true);
		}

		private async Task SendDeathSequenceAsync(IMqttClient mqttClient)
		{
			await SendStatusAsync(mqttClient, false);
		}

		private async Task SendStatusAsync(IMqttClient mqttClient, bool state)
		{
			var payload = new { online = state, timestamp = _connectTimestamp };
			await mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(SparkplugMessageType.STATE, _nodeName).ToMqttTopic())
				.WithPayload(JsonSerializer.Serialize(payload))
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
				.WithRetainFlag(true)
				.Build());
		}

		public async Task SendNodeCommandAsync(string node, Payload payload)
		{
			payload = PreparePayloadForTransmission(payload);
			await _mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(SparkplugMessageType.NCMD, node, _group).ToMqttTopic())
				.WithPayload(payload.ToByteArray())
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
				.Build());
		}

		public async Task SendDeviceCommandAsync(string node, string device, Payload payload)
		{
			payload = PreparePayloadForTransmission(payload);
			await _mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(SparkplugMessageType.DCMD, node, _group, device).ToMqttTopic())
				.WithPayload(payload.ToByteArray())
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
				.Build());
		}

	}
}
