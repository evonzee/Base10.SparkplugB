using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Enums;
using Base10.SparkplugB.Core.Internal;
using FluentAssertions;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Internal
{
	public class SparkplugTopicParserTests
	{
		[Theory]
		[InlineData("spBv1.0/group/DCMD/edge_node/device_id", "group", CommandType.DCMD, "edge_node", "device_id")]
		[InlineData("spBv1.0/group/NCMD/edge_node", "group", CommandType.NCMD, "edge_node", null)]
		[InlineData("spBv1.0/STATE/edge_node", null, CommandType.STATE, "edge_node", null)]
		public void BasicValidTopicsParse(string topic, string group, CommandType command, string node, string deviceId)
		{
			var parser = new SparkplugTopicParser();
			var parsed = parser.Parse(topic);

			parsed.Should().NotBeNull();
			parsed.Command.Should().Be(command);
			parsed.Node.Should().Be(node);
			parsed.Group.Should().Be(group);
			parsed.DeviceId.Should().Be(deviceId);
		}
	}
}
