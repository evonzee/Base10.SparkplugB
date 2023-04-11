using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MQTTnet.Client;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class MqttConnectionManagementTests
	{
		[Fact]
		public void ServiceConnectsOk()
		{
			var mqttClient = new Mock<IMqttClient>();
			mqttClient.SetupAdd(m => m.ConnectedAsync += It.IsAny<Func<MqttClientConnectedEventArgs, Task>>()).Verifiable();
			mqttClient.SetupAdd(m => m.DisconnectedAsync += It.IsAny<Func<MqttClientDisconnectedEventArgs, Task>>()).Verifiable();
			mqttClient.SetupAdd(m => m.ApplicationMessageReceivedAsync += It.IsAny<Func<MqttApplicationMessageReceivedEventArgs, Task>>()).Verifiable();
			mqttClient.Setup<Task<MqttClientConnectResult>>(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();

			var connects = 0;
			var app = new ExposedSparkplugMqttService(mqttClient.Object);
			app.Connected += (e) => { ++connects; return Task.CompletedTask; };
			app.Connect().Wait();

			mqttClient.Verify();
			connects.Should().Be(1);
		}

		[Fact]
		public void ServiceReconnectsOnDisconnect()
		{
			var mqttClient = new Mock<IMqttClient>();
			mqttClient.Setup<Task<MqttClientConnectResult>>(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();

			var app = new ExposedSparkplugMqttService(mqttClient.Object);

			app.Connect().Wait();

			mqttClient.Raise(m => m.DisconnectedAsync += null, new object[] { new MqttClientDisconnectedEventArgs()});

			mqttClient.Verify(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
			mqttClient.Verify();
			app.CurrentBirthSequence().Should().Be(1);
		}

		[Fact]
		public void ServicDoesNotReconnectAfterManualDisconnect()
		{
			var mqttClient = new Mock<IMqttClient>();
			mqttClient.Setup<Task<MqttClientConnectResult>>(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();

			mqttClient.Setup<Task>(m => m.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask)
				.Raises(m => m.DisconnectedAsync += null, new object[] { new MqttClientDisconnectedEventArgs() })
				.Verifiable();

			var app = new ExposedSparkplugMqttService(mqttClient.Object);

			app.Connect().Wait();
			app.Disconnect().Wait();

			mqttClient.Verify(m => m.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
			mqttClient.Verify();
			app.CurrentBirthSequence().Should().Be(0);
		}
	}
}
