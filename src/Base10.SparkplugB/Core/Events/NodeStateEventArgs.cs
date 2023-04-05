using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;

namespace Base10.SparkplugB.Core.Events
{
	public class NodeStateEventArgs : EventArgs
	{
		public NodeStateEventArgs(SparkplugTopic topic, NodeState state)
		{
			Topic = topic;
			State = state;
		}

		public SparkplugTopic Topic { get; }
		public NodeState State { get; }
	}
}
