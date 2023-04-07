using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Enums;

namespace Base10.SparkplugB.Core.Data
{
	public record SparkplugTopic(CommandType Command, string Node, string? Group = null, string? DeviceId = null)
	{
		public override string ToString()
		{
			return $"{Command} for {Group}/{Node}/{DeviceId}";
		}
	}
}
