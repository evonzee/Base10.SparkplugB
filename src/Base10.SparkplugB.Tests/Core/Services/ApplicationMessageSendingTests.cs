using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Services;
using Base10.SparkplugB.Protocol;
using FluentAssertions;
using Moq;
using MQTTnet.Client;
using Xunit;
using static Base10.SparkplugB.Protocol.Payload.Types;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class ApplicationMessageSendingTests
	{
		[Theory]
		[MemberData(nameof(GenerateMetrics), 0)]
		[MemberData(nameof(GenerateMetrics), 1)]
		[MemberData(nameof(GenerateMetrics), 2)]
		public async Task MessagesSetSeqBdSeqAndUseCorrectTopic(IList<Metric> metrics)
		{
			var mqttClient = new Mock<IMqttClient>();
			MQTTnet.MqttApplicationMessage? message = null;
			mqttClient.Setup(x => x.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
				.Callback<MQTTnet.MqttApplicationMessage, CancellationToken>((m, _) => message = m)
				.Returns(Task.FromResult(new MqttClientPublishResult()));


			var app = new SparkplugApplication(new SparkplugServiceOptions() { Group = "testgroup" }, mqttClient.Object);

			Payload payload = new()
			{
				Seq = 0,
			};
			payload.Metrics.AddRange(metrics);
			await app.SendDeviceCommand("node", "device", payload);

			message.Should().NotBeNull();
			var p = Payload.Parser.ParseFrom(message?.Payload);
			p.Metrics.Count.Should().Be(metrics.Count + 1);
			p.Metrics.Any(m => m.Name == "bdSeq").Should().BeTrue();
			p.Metrics.Where(m => m.Name == "bdSeq").First().IntValue.Should().Be(app.CurrentBirthSequence());
			message?.Topic.Should().Be("spBv1.0/testgroup/DCMD/node/device");

			// send it again to make sure sequence increments
			var lastSequence = payload.Seq;
			await app.SendDeviceCommand("node", "device", payload);
			payload.Seq.Should().Be(lastSequence + 1);
			p.Metrics.Count.Should().Be(metrics.Count + 1); // make sure duplicate bdSeq is not added

			// send it again as a node metric
			await app.SendNodeCommand("node", payload);
			payload.Seq.Should().Be(lastSequence + 2);
			p.Metrics.Count.Should().Be(metrics.Count + 1);
			message?.Topic.Should().Be("spBv1.0/testgroup/NCMD/node");
		}

		public static IEnumerable<object[]> GenerateMetrics(int count)
		{
			var metrics = new List<Metric>();
			for (int i = 0; i < count; i++)
			{
				metrics.Add(new Metric()
				{
					Name = $"metric{i}",
					StringValue = i.ToString(),
					Datatype = (uint)DataType.Int64,
				});
			}

			yield return new object[] { metrics };
		}
	}
}
