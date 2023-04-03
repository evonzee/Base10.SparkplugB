using System;
using MQTTnet;

namespace Base10.SparkplugB.Core.Internal
{
	public class SparkplugParser
	{
		private readonly SparkplugTopicParser _topicParser;

		public SparkplugParser() : this(new SparkplugTopicParser()) { }
		public SparkplugParser(SparkplugTopicParser topicParser)
		{
			_topicParser = topicParser;
		}

		public object Parse(MqttApplicationMessage applicationMessage)
		{
			throw new NotImplementedException();
		}
	}
}
