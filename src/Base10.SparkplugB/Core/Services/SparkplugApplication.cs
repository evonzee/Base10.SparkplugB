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
			this.Connected += OnConnected;
			this.BeforeDisconnect += OnBeforeDisconnect;
		}

		private async Task OnConnected(EventArgs e)
		{
			try
			{
				// subscribe to appropriate topics per configuration
				// subscribe happens before birth, per [tck-id-host-topic-phid-birth-required]
				await SubscribeInitial(_mqttClient);

				// send birth message
				await SendBirthSequence(_mqttClient); // apps must satisfy [tck-id-components-ph-state]
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to startup application!  Behavior may be somewhat strange.");
				throw;
			}
		}

		protected async Task OnBeforeDisconnect(EventArgs e)
		{
			await SendDeathSequence(_mqttClient);
		}

		protected override MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder builder)
		{
			_connectTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var willPayload = new { online = false, timestamp = _connectTimestamp }; // [tck-id-host-topic-phid-death-payload-connect]
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
			var optionsBuilder = new MqttClientSubscribeOptionsBuilder()
				.WithTopicFilter(CommandType.STATE.GetSubscriptionPattern(), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			if (_options.Promiscuous)
			{
				optionsBuilder = optionsBuilder.WithTopicFilter("spBv1.0/#");
			}
			else
			{
				optionsBuilder = optionsBuilder.WithTopicFilter(CommandType.NBIRTH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(CommandType.NDEATH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(CommandType.NDATA.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(CommandType.DBIRTH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(CommandType.DDEATH.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
					.WithTopicFilter(CommandType.DDATA.GetSubscriptionPattern(_group), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
			}

			var options = optionsBuilder.Build();
			await mqttClient.SubscribeAsync(options);
		}

		private async Task SendBirthSequence(IMqttClient mqttClient)
		{
			await SendStatus(mqttClient, true);
		}

		private async Task SendDeathSequence(IMqttClient mqttClient)
		{
			await SendStatus(mqttClient, false);
		}

		private async Task SendStatus(IMqttClient mqttClient, bool state)
		{
			var payload = new { online = state, timestamp = _connectTimestamp };
			await mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(CommandType.STATE, _nodeName).ToMqttTopic())
				.WithPayload(JsonSerializer.Serialize(payload))
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
				.WithRetainFlag(true)
				.Build());
		}

		public async Task SendNodeCommand(string node, Payload payload)
		{
			payload = PreparePayloadForTransmission(payload);
			await _mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(CommandType.NCMD, node, _group).ToMqttTopic())
				.WithPayload(payload.ToByteArray())
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
				.Build());
		}

		public async Task SendDeviceCommand(string node, string device, Payload payload)
		{
			payload = PreparePayloadForTransmission(payload);
			await _mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
				.WithTopic(new SparkplugTopic(CommandType.DCMD, node, _group, device).ToMqttTopic())
				.WithPayload(payload.ToByteArray())
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
				.Build());
		}

	}
}
