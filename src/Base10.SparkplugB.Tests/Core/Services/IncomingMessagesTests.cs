using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Enums;
using Base10.SparkplugB.Core.Events;
using Base10.SparkplugB.Protocol;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class IncomingMessagesTests
	{
		[Theory]
		[InlineData("{\"online\": true, \"timestamp\": 1680702074814}", true, "2023-04-05 13:41:15")]
		[InlineData("{\"online\": false, \"timestamp\": 0}", false, "1970-01-01 00:00:00")]
		public async Task ValidStateMessagesRaiseEvents(string message, bool online, string timestamp)
		{
			var service = new ExposedSparkplugMqttService();
			NodeStateEventArgs results = null;
			service.StateMessageReceived += async (eventArgs) => results = eventArgs;
			service.InvalidMessageReceived += (args) => throw new Exception("Invalid message handler should not be called!");

			var args = new MqttApplicationMessageReceivedEventArgs(
				"test",
				new MqttApplicationMessageBuilder()
					.WithTopic("spBv1.0/STATE/bob")
					.WithPayload(message)
					.Build()
				, new MQTTnet.Packets.MqttPublishPacket()
				, null
			);
			Func<Task> action = async () => await service.OnMessageReceived(args);
			await action.Should().NotThrowAsync();
			results.Should().NotBeNull();
			results.Topic.Node.Should().Be("bob");
			results.State.Online.Should().Be(online);
			results.State.TimestampAsDateTime.Should().BeCloseTo(DateTime.Parse(timestamp), TimeSpan.FromSeconds(1));
		}

		[Theory]
		[InlineData("spBv1.0/STATE/bob", "{\"online\": \"true\", \"timestamp\": 1680702074814}")]
		[InlineData("spBv1.0/STATE/bob", "trash")]
		[InlineData("spBv1.0/asdf/bob", "trash")]
		public async Task InvalidStateMessagesRaiseEvents(string topic, string message)
		{
			var service = new ExposedSparkplugMqttService();
			InvalidMessageReceivedEventEventArgs results = null;
			service.StateMessageReceived += (args) => throw new Exception("Valid message handler should not be called!");
			service.InvalidMessageReceived += async (eventArgs) => results = eventArgs;

			var args = new MqttApplicationMessageReceivedEventArgs(
				"test",
				new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(message)
					.Build()
				, new MQTTnet.Packets.MqttPublishPacket()
				, null
			);
			Func<Task> action = async () => await service.OnMessageReceived(args);
			await action.Should().NotThrowAsync();
			results.Should().NotBeNull();
			results.Topic.Should().Be(topic);
			results.Payload.Should().Equal(System.Text.Encoding.UTF8.GetBytes(message));
		}

		[Theory]
		[InlineData("spBv1.0/validsparkplug/NBIRTH/nodeid", new byte[] { 0x10, 0x0A }, "validsparkplug", "nodeid", null, CommandType.NBIRTH)]
		public async Task ValidSparkplugMessagesParseOk(string topic, byte[] payload, string group, string node, string device, CommandType type)
		{
			var service = new ExposedSparkplugMqttService();
			SparkplugEventArgs results = null;
			service.NodeBirthReceived += async (eventArgs) => results = eventArgs;
			service.InvalidMessageReceived += (args) => throw new Exception("Invalid message handler should not be called!");

			var args = new MqttApplicationMessageReceivedEventArgs(
				"test",
				new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(payload)
					.Build()
				, new MQTTnet.Packets.MqttPublishPacket()
				, null
			);
			Func<Task> action = async () => await service.OnMessageReceived(args);
			await action.Should().NotThrowAsync();
			results.Should().NotBeNull();
			results.Topic.Node.Should().Be(node);
			results.Topic.Group.Should().Be(group);
			results.Topic.DeviceId.Should().Be(device);
			results.Topic.Command.Should().Be(type);
		}

	}
}
