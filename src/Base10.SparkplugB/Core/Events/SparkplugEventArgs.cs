using System;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Protocol;

namespace Base10.SparkplugB.Core.Events
{
	public class SparkplugEventArgs : EventArgs
	{
		public SparkplugEventArgs(SparkplugTopic topic, Payload payload)
		{
			Topic = topic;
			Payload = payload;
		}

		public SparkplugTopic Topic { get; }
		public Payload Payload { get; }
	}
}
