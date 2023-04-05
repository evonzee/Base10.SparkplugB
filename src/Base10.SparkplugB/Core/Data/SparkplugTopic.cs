using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Enums;

namespace Base10.SparkplugB.Core.Data
{
	public readonly struct SparkplugTopic
	{
		public SparkplugTopic(CommandType command, string node, string? group = null, string? deviceId = null)
		{
			Command = command;
			Node = node ?? throw new ArgumentNullException(nameof(node)); ;
			Group = group;
			DeviceId = deviceId;
		}
		public readonly CommandType Command { get; }
		public readonly string Node { get; }
		public readonly string? Group { get; }
		public readonly string? DeviceId { get; }
	}
}
