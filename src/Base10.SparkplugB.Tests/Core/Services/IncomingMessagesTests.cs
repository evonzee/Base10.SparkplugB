using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Events;
using Base10.SparkplugB.Core.Services;
using FluentAssertions;
using Moq;
using MQTTnet;
using MQTTnet.Client;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class IncomingMessagesTests
	{
		[Theory]
		[InlineData("{\"online\": true, \"timestamp\": 1680702074814}", true, "2023-04-05 13:41:15")]
		[InlineData("{\"online\": false, \"timestamp\": 0}", false, "1970-01-01 00:00:00")]
		public async Task ValidStateMessagesRaiseEvents(string message, bool online, string timestamp)
		{
			var mqttClient = new Mock<IMqttClient>();
			Func<MqttApplicationMessageReceivedEventArgs, Task> callback = (args) => throw new Exception("Callback not set!");
			mqttClient.SetupAdd(x => x.ApplicationMessageReceivedAsync += It.IsAny<Func<MqttApplicationMessageReceivedEventArgs, Task>>())
				.Callback<Func<MqttApplicationMessageReceivedEventArgs, Task>>(x => callback = x);

			var service = new ExposedSparkplugMqttService(new SparkplugServiceOptions() { Group = "testgroup" }, mqttClient.Object);
			NodeStateEventArgs? results = null;
			service.StateMessageReceivedAsync += (eventArgs) => { results = eventArgs; return Task.CompletedTask; };
			service.InvalidMessageReceivedAsync += (args) => throw new Exception("Invalid message handler should not be called!");

			var args = new MqttApplicationMessageReceivedEventArgs(
				"test",
				new MqttApplicationMessageBuilder()
					.WithTopic("spBv1.0/STATE/bob")
					.WithPayload(message)
					.Build()
				, new MQTTnet.Packets.MqttPublishPacket()
				, null
			);
			Func<Task> action = async () => await callback(args);
			await action.Should().NotThrowAsync();
			results.Should().NotBeNull();
			results?.Topic.Node.Should().Be("bob");
			results?.State.Online.Should().Be(online);
			results?.State.TimestampAsDateTime.Should().BeCloseTo(DateTime.Parse(timestamp), TimeSpan.FromSeconds(1));
		}

		[Theory]
		[InlineData("spBv1.0/STATE/bob", "{\"online\": \"true\", \"timestamp\": 1680702074814}")]
		[InlineData("spBv1.0/STATE/bob", "trash")]
		[InlineData("spBv1.0/asdf/bob", "trash")]
		public async Task InvalidStateMessagesRaiseEvents(string topic, string message)
		{
			var mqttClient = new Mock<IMqttClient>();
			Func<MqttApplicationMessageReceivedEventArgs, Task> callback = (args) => throw new Exception("Callback not set!");
			mqttClient.SetupAdd(x => x.ApplicationMessageReceivedAsync += It.IsAny<Func<MqttApplicationMessageReceivedEventArgs, Task>>())
				.Callback<Func<MqttApplicationMessageReceivedEventArgs, Task>>(x => callback = x);

			var service = new ExposedSparkplugMqttService(new SparkplugServiceOptions() { Group = "testgroup" }, mqttClient.Object);
			InvalidMessageReceivedEventEventArgs? results = null;
			service.StateMessageReceivedAsync += (args) => throw new Exception("Valid message handler should not be called!");
			service.InvalidMessageReceivedAsync += (eventArgs) => { results = eventArgs; return Task.CompletedTask; };

			var args = new MqttApplicationMessageReceivedEventArgs(
				"test",
				new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(message)
					.Build()
				, new MQTTnet.Packets.MqttPublishPacket()
				, null
			);

			Func<Task> action = async () => await callback(args);
			await action.Should().NotThrowAsync();
			results.Should().NotBeNull();
			results?.Topic.Should().Be(topic);
			results?.Payload.Should().Equal(System.Text.Encoding.UTF8.GetBytes(message));
		}

		// 0x180A is an empty message with sequence 10
		[Theory]
		[InlineData("spBv1.0/validsparkplug/NBIRTH/nodeid", new byte[] { 0x18, 0x0A }, "NodeBirthReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/NDATA/nodeid", new byte[] { 0x18, 0x0A }, "NodeDataReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/NDEATH/nodeid", new byte[] { 0x18, 0x0A }, "NodeDeathReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/NCMD/nodeid", new byte[] { 0x18, 0x0A }, "NodeCommandReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/DBIRTH/nodeid/deviceid", new byte[] { 0x18, 0x0A }, "DeviceBirthReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/DDATA/nodeid/deviceid", new byte[] { 0x18, 0x0A }, "DeviceDataReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/DDEATH/nodeid/deviceid", new byte[] { 0x18, 0x0A }, "DeviceDeathReceivedAsync")]
		[InlineData("spBv1.0/validsparkplug/DCMD/nodeid/deviceid", new byte[] { 0x18, 0x0A }, "DeviceCommandReceivedAsync")]
		public async Task ValidSparkplugMessagesParseOk(string topic, byte[] payload, string eventName)
		{
			var mqttClient = new Mock<IMqttClient>();
			Func<MqttApplicationMessageReceivedEventArgs, Task> callback = (args) => throw new Exception("Callback not set!");
			mqttClient.SetupAdd(x => x.ApplicationMessageReceivedAsync += It.IsAny<Func<MqttApplicationMessageReceivedEventArgs, Task>>())
				.Callback<Func<MqttApplicationMessageReceivedEventArgs, Task>>(x => callback = x);

			var service = new ExposedSparkplugMqttService(new SparkplugServiceOptions() { Group = "testgroup" }, mqttClient.Object);
			SparkplugEventArgs? results = null;
			Func<SparkplugEventArgs, Task> handler = (eventArgs) => { results = eventArgs; return Task.CompletedTask; };
			service.GetType().GetEvent(eventName)?.AddEventHandler(service, handler);
			service.InvalidMessageReceivedAsync += (args) => throw new Exception("Invalid message handler should not be called!");

			var args = new MqttApplicationMessageReceivedEventArgs(
				"test",
				new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(payload)
					.Build()
				, new MQTTnet.Packets.MqttPublishPacket()
				, null
			);

			Func<Task> action = async () => await callback(args);
			await action.Should().NotThrowAsync();
			results.Should().NotBeNull();
			results?.Payload.Seq.Should().Be(10);
		}

	}
}
