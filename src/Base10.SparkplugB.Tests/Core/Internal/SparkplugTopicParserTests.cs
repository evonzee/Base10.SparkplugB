using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Enums;
using Base10.SparkplugB.Core.Internal;
using FluentAssertions;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Internal
{
	public class SparkplugTopicParserTests
	{
		[Theory]
		[InlineData("spBv1.0/group/DCMD/edge_node/device_id", "group", SparkplugMessageType.DCMD, "edge_node", "device_id")]
		[InlineData("spBv1.0/group/NCMD/edge_node", "group", SparkplugMessageType.NCMD, "edge_node", null)]
		[InlineData("spBv1.0/STATE/edge_node", null, SparkplugMessageType.STATE, "edge_node", null)]
		public void BasicValidTopicsParse(string topic, string group, SparkplugMessageType command, string node, string deviceId)
		{
			var parser = new SparkplugTopicParser();
			var parsed = parser.Parse(topic);

			parsed.Should().NotBeNull();
			parsed.Command.Should().Be(command);
			parsed.Node.Should().Be(node);
			parsed.Group.Should().Be(group);
			parsed.DeviceId.Should().Be(deviceId);
		}

		[Theory]
		[InlineData("nonsparkplug")]
		[InlineData("spBv1.0/asdf/adf/asdf")]
		[InlineData("spBv1.0/asdf/STATE/asdf")]
		public void InvalidTopicsThrow(string topic)
		{
			var parser = new SparkplugTopicParser();
			Action act = () => parser.Parse(topic);

			act.Should().Throw<ArgumentException>();
		}

		[Theory]
		[InlineData("spBv1.0/group/DCMD/edge_node/device_id")]
		public void RepeatTopicsAreCached(string topic)
		{
			var results = new List<SparkplugTopic>();
			var parser = new SparkplugTopicParser();
			for (var i = 0; i < 10; i++)
			{
				results.Add(parser.Parse(topic));
			}
			var firstResult = results.First();
			results.Should().AllBeEquivalentTo(firstResult);
		}
	}
}
