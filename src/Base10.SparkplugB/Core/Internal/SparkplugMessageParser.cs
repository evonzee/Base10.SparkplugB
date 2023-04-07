using System;
using Base10.SparkplugB.Core.Data;
using System.Text.Json;
using Base10.SparkplugB.Protocol;

namespace Base10.SparkplugB.Core.Internal
{
	public class SparkplugMessageParser
	{
		private readonly JsonSerializerOptions _options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
		};
		public NodeState? ParseState(byte[] applicationMessage)
		{
			return JsonSerializer.Deserialize<NodeState>(applicationMessage, _options);
		}

		public Payload ParseSparkplug(byte[] applicationMessage)
		{
			return Payload.Parser.ParseFrom(applicationMessage);
		}
	}
}
