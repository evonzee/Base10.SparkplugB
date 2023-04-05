using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Internal;
using FluentAssertions;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Internal
{
	public class SparkplugMessageParserTests
	{
		[Theory]
		[InlineData("{\"online\": true, \"timestamp\": 1680702074814}", true, "2023-04-05 13:41:15")]
		[InlineData("{\"online\": false, \"timestamp\": 1680702074814}", false, "2023-04-05 13:41:15")]
		[InlineData("{\"online\": true, \"timestamp\": 0}", true, "1970-01-01 00:00:00")]
		public void ValidStateMessagesParseOk(string payload, bool online, string timestamp)
		{
			var parser = new SparkplugMessageParser();
			// convert the payload to a byte array in utf-8 format
			var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
			var state = parser.ParseState(bytes);
			state.Should().NotBeNull();
			state?.Online.Should().Be(online);
			state?.TimestampAsDateTime.Should().BeCloseTo(DateTime.Parse(timestamp), TimeSpan.FromSeconds(5));
		}

		[Theory]
		[InlineData("{\"online\": true, \"timestamp\": \"joe\"}")]
		[InlineData("ONLINE")]
		[InlineData("{\"online\": \"true\", \"timestamp\": 0}")]
		[InlineData("{\"online\": true, \"timestamp\": \"0\"}")]
		public void InvalidStateMessagesDontParse(string payload)
		{
			var parser = new SparkplugMessageParser();
			// convert the payload to a byte array in utf-8 format
			var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
			Action act = () => parser.ParseState(bytes);

			act.Should().Throw<JsonException>();
		}
	}
}
