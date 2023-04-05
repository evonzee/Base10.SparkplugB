using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;

namespace Base10.SparkplugB.Core.Events
{
	public class InvalidMessageReceivedEventEventArgs : EventArgs
	{
		public InvalidMessageReceivedEventEventArgs(string topic, byte[] payload)
		{
			Topic = topic;
			Payload = payload;
		}

		public string Topic { get; }
		public byte[] Payload { get; }
	}
}
