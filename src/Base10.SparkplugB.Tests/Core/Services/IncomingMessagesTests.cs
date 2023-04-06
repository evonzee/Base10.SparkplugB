using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Events;
using FluentAssertions;
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
	}
}
