using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Enums;

namespace Base10.SparkplugB.Core.Data
{
	public record SparkplugTopic(SparkplugMessageType Command, string Node, string? Group = null, string? DeviceId = null)
	{
		public string ToMqttTopic()
		{
			return Command switch
			{
				SparkplugMessageType.STATE => $"spBv1.0/STATE/{Node}",
				SparkplugMessageType.NBIRTH or SparkplugMessageType.NDATA or SparkplugMessageType.NDEATH or SparkplugMessageType.NCMD => $"spBv1.0/{Group}/{Command}/{Node}",
				SparkplugMessageType.DBIRTH or SparkplugMessageType.DDATA or SparkplugMessageType.DDEATH or SparkplugMessageType.DCMD => $"spBv1.0/{Group}/{Command}/{Node}/{DeviceId}",
				_ => throw new ArgumentException($"Command '{Command}' is not a valid Sparkplug command", nameof(Command)),
			};
		}
		public override string ToString()
		{
			return $"{Command} for {Group}/{Node}/{DeviceId}";
		}
	}
}
