using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Services;
using FluentAssertions;
using Moq;
using MQTTnet.Client;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class BasicSparkplugMqttServiceTests
	{
		[Fact]
		public void CommandSequenceNumberIsThreadSafe()
		{
			var app = new Mock<ExposedSparkplugMqttService>()
			{
				CallBase = true
			}.Object;
			var tasks = new List<Task>();
			for (int i = 0; i < 100; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					for (int j = 0; j < 100; j++)
					{
						app.NextCommandSequence();
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());
			app.NextCommandSequence().Should().Be(10000 % 256);
		}

		[Fact]
		public void BirthSequenceNumberIsThreadSafe()
		{
			var app = new Mock<ExposedSparkplugMqttService>()
			{
				CallBase = true
			}.Object;

			app.NextBirthSequence();
			app.CurrentBirthSequence().Should().Be(0);

			var tasks = new List<Task>();
			for (int i = 0; i < 100; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					for (int j = 0; j < 100; j++)
					{
						app.NextBirthSequence();
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());
			app.CurrentBirthSequence().Should().Be(10000 % 256);
		}

		[Fact]
		public void ResettingBirthSequenceWorks()
		{
			var app = new Mock<ExposedSparkplugMqttService>()
			{
				CallBase = true
			}.Object;

			ulong seq = app.NextCommandSequence();
			seq.Should().Be(0);

			app.NextCommandSequence();
			app.NextCommandSequence();
			app.NextCommandSequence();
			app.ResetCommandSequence();
			seq = app.NextCommandSequence();
			seq.Should().Be(0);
		}

		[Fact]
		public async Task BasicEventsAreFired()
		{
			var mqttClient = new Mock<IMqttClient>();
			mqttClient.Setup(x => x.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new MqttClientConnectResult()))
				.Raises(m => m.ConnectedAsync += null, new object[] { new MqttClientConnectedEventArgs(new MqttClientConnectResult()) })
				.Verifiable();

			mqttClient.Setup(x => x.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask)
				.Raises(m => m.DisconnectedAsync += null, new object[] { new MqttClientDisconnectedEventArgs() })
				.Verifiable();

			var app = new Mock<ExposedSparkplugMqttService>(mqttClient.Object){ CallBase = true }.Object;

			// setup fire recording and a set of handlers to add/remove
			var status = new Dictionary<string, bool>();
			var handlers = new Dictionary<string, Func<EventArgs, Task>>
			{
				{"BeforeStart", (args) => { status.Add("BeforeStart", true); return Task.CompletedTask;}},
				{"Started", (args) => { status.Add("Started", true); return Task.CompletedTask;}},
				{"Connected", (args) => { status.Add("Connected", true); return Task.CompletedTask;}},
				{"BeforeDisconnect", (args) => { status.Add("BeforeDisconnect", true); return Task.CompletedTask;}},
				{"Disconnected", (args) => { status.Add("Disconnected", true); return Task.CompletedTask;}},
			};
			app.BeforeStart += handlers["BeforeStart"];
			app.Started += handlers["Started"];
			app.Connected += handlers["Connected"];
			app.BeforeDisconnect += handlers["BeforeDisconnect"];
			app.Disconnected += handlers["Disconnected"];

			await app.Connect();
			status.Should().Contain("BeforeStart", true);
			status.Should().Contain("Started", true);
			status.Should().Contain("Connected", true);
			status.Should().NotContainKey("BeforeDisconnect");
			status.Should().NotContainKey("Disconnected");

			await app.Disconnect();
			status.Should().Contain("BeforeDisconnect", true);
			status.Should().Contain("Disconnected", true);

			// confirm that we can remove all the handlers
			app.BeforeStart -= handlers["BeforeStart"];
			app.Started -= handlers["Started"];
			app.Connected -= handlers["Connected"];
			app.BeforeDisconnect -= handlers["BeforeDisconnect"];
			app.Disconnected -= handlers["Disconnected"];

			status.Clear();
			await app.Connect();
			await app.Disconnect();
			status.Should().BeEmpty();
		}
	}
}
