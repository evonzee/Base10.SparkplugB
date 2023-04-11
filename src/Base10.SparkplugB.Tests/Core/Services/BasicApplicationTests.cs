using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Services;
using FluentAssertions;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Base10.SparkplugB.Tests.Core.Services
{
    public class BasicApplicationTests
    {
        [Fact]
        public async Task ApplicationCompliesWithStartupSpec()
        {
			var mqttClient = new Mock<IMqttClient>();
			mqttClient
				.Setup<Task<MqttClientConnectResult>>(m => m.ConnectAsync(It.Is<MqttClientOptions>(
					o => o.CleanSession == true
					&& o.WillPayload.Length > 10 // don't check exactly, but check that it has some value
					&& o.WillTopic == $"spBv1.0/STATE/SomeNode"
					&& o.WillRetain
					&& o.WillQualityOfServiceLevel == MqttQualityOfServiceLevel.AtLeastOnce
				), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();

			var app = new SparkplugApplication(new SparkplugServiceOptions(){
				Group = "SomeGroup",
				NodeName = "SomeNode"
			}, mqttClient.Object);
			await app.Connect();

			mqttClient.Verify();
			mqttClient.Verify(
				m => m.SubscribeAsync(
					It.Is<MqttClientSubscribeOptions>(
						o => o.TopicFilters.Count(f => f.Topic.StartsWith("spBv1.0/SomeGroup") && f.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce) == 6
						&& o.TopicFilters.Count(f => f.Topic.StartsWith("spBv1.0/STATE") && f.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce) == 1
						&& o.TopicFilters.Count == 7
					),
					It.IsAny<CancellationToken>()
				));

			mqttClient.Verify(
					m => m.PublishAsync(It.Is<MqttApplicationMessage>(
						m => m.Topic == "spBv1.0/STATE/SomeNode"
						&& m.ConvertPayloadToString().Contains("true")
				), It.IsAny<CancellationToken>())
			);
        }

		[Fact]
		public async Task GracefulDisconnectsSendDeath()
		{
			var mqttClient = new Mock<IMqttClient>();

			mqttClient
				.Setup<Task<MqttClientConnectResult>>(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();


			var app = new SparkplugApplication(new SparkplugServiceOptions(){
				Group = "SomeGroup",
				NodeName = "SomeNode"
			}, mqttClient.Object);
			await app.Connect();
			await app.Disconnect();

			mqttClient.Verify();
			mqttClient.Verify(
					m => m.PublishAsync(It.Is<MqttApplicationMessage>(
						m => m.Topic == "spBv1.0/STATE/SomeNode"
						&& m.ConvertPayloadToString().Contains("false")
				), It.IsAny<CancellationToken>())
			);
		}

		[Fact]
		public async Task UnexpectedDisconnectRebirths()
		{
			var mqttClient = new Mock<IMqttClient>();

			mqttClient
				.Setup<Task<MqttClientConnectResult>>(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();


			var app = new SparkplugApplication(new SparkplugServiceOptions(){
				Group = "SomeGroup",
				NodeName = "SomeNode"
			}, mqttClient.Object);
			await app.Connect();

			mqttClient.Raise(m => m.DisconnectedAsync += null, new object[] { new MqttClientDisconnectedEventArgs()});

			mqttClient.Verify();
			mqttClient.Verify(
				m => m.PublishAsync(
						It.Is<MqttApplicationMessage>(
							m => m.Topic == "spBv1.0/STATE/SomeNode"
							&& m.ConvertPayloadToString().Contains("true")
					), It.IsAny<CancellationToken>()
				),
				Times.Exactly(2)
			);
		}

    }
}
