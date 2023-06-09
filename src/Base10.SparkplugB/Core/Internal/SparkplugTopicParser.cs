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

			var commandType = Enum.Parse<SparkplugMessageType>(topicParts[2], true);

			switch (commandType)
			{
				case SparkplugMessageType.NBIRTH:
				case SparkplugMessageType.NDATA:
				case SparkplugMessageType.NDEATH:
				case SparkplugMessageType.NCMD:
					return ParseNodeCommand(topicParts, commandType);
				case SparkplugMessageType.DBIRTH:
				case SparkplugMessageType.DDATA:
				case SparkplugMessageType.DDEATH:
				case SparkplugMessageType.DCMD:
					return ParseDeviceCommand(topicParts, commandType);
				default:
					throw new ArgumentException($"Topic '{topic}' is not a valid Sparkplug topic: STATE commands must not include group.", nameof(topic));
			}
		}


		private SparkplugTopic ParseState(string[] topicParts)
		{
			return new SparkplugTopic(Command: SparkplugMessageType.STATE, Node: topicParts[2]);
		}

		private SparkplugTopic ParseNodeCommand(string[] topicParts, SparkplugMessageType commandType)
		{
			return new SparkplugTopic(Command: commandType, Node: topicParts[3], Group: topicParts[1]);
		}

		private SparkplugTopic ParseDeviceCommand(string[] topicParts, SparkplugMessageType commandType)
		{
			return new SparkplugTopic(Command: commandType, Node: topicParts[3], Group: topicParts[1], DeviceId: topicParts[4]);
		}

	}
}
