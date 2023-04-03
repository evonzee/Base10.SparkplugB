using System;
using System.Collections.Generic;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Enums;

namespace Base10.SparkplugB.Core.Internal
{
	public class SparkplugTopicParser
	{
		private Dictionary<string, SparkplugTopic> _cache = new Dictionary<string, SparkplugTopic>();

		public SparkplugTopic Parse(string topic)
		{
			if (_cache.TryGetValue(topic, out var cached))
				return cached;

			// parse and save so we don't have to reparse often
			var parsed = ParseInternal(topic);
			_cache[topic] = parsed;
			return parsed;
		}

		public SparkplugTopic ParseInternal(string topic)
		{
			var topicParts = topic.Split('/');
			if (topicParts.Length < 3 || topicParts[0] != "spBv1.0")
				throw new ArgumentException($"Topic '{topic}' is not a valid Sparkplug topic", nameof(topic));

			if (topicParts[1].Equals("STATE"))
				return ParseState(topicParts);

			var commandType = Enum.Parse<CommandType>(topicParts[2], true);

			switch (commandType)
			{
				case CommandType.NBIRTH:
				case CommandType.NDATA:
				case CommandType.NDEATH:
				case CommandType.NCMD:
					return ParseNodeCommand(topicParts, commandType);
				case CommandType.DBIRTH:
				case CommandType.DDATA:
				case CommandType.DDEATH:
				case CommandType.DCMD:
					return ParseDeviceCommand(topicParts, commandType);
				default:
					throw new ArgumentException($"Topic '{topic}' is not a valid Sparkplug topic", nameof(topic));
			}
		}


		private SparkplugTopic ParseState(string[] topicParts)
		{
			return new SparkplugTopic(command: CommandType.STATE, node: topicParts[2]);
		}
		private SparkplugTopic ParseNodeCommand(string[] topicParts, CommandType commandType)
		{
			return new SparkplugTopic(command: commandType, node: topicParts[3], group: topicParts[1]);
		}
		private SparkplugTopic ParseDeviceCommand(string[] topicParts, CommandType commandType)
		{
			return new SparkplugTopic(command: commandType, node: topicParts[3], group: topicParts[1], deviceId: topicParts[4]);
		}

	}
}
