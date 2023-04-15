using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Enums;

namespace Base10.SparkplugB.Core.Data
{
	public record SparkplugTopic(CommandType Command, string Node, string? Group = null, string? DeviceId = null)
	{
		public string ToMqttTopic()
		{
			return Command switch
			{
				CommandType.STATE => $"spBv1.0/STATE/{Node}",
				CommandType.NBIRTH or CommandType.NDATA or CommandType.NDEATH or CommandType.NCMD => $"spBv1.0/{Group}/{Command}/{Node}",
				CommandType.DBIRTH or CommandType.DDATA or CommandType.DDEATH or CommandType.DCMD => $"spBv1.0/{Group}/{Command}/{Node}/{DeviceId}",
				_ => throw new ArgumentException($"Command '{Command}' is not a valid Sparkplug command", nameof(Command)),
			};
		}
		public override string ToString()
		{
			return $"{Command} for {Group}/{Node}/{DeviceId}";
		}
	}
}
