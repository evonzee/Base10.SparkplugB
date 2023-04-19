using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Core.Enums
{
	public static class SparkplugMessageTypeTypeMethods
	{
		public static string GetSubscriptionPattern(this SparkplugMessageType commandType, string? group = null)
		{
			return commandType switch
			{
				SparkplugMessageType.STATE => $"spBv1.0/STATE/#",
				SparkplugMessageType.NBIRTH or SparkplugMessageType.NDATA or SparkplugMessageType.NDEATH or SparkplugMessageType.NCMD or SparkplugMessageType.DBIRTH or SparkplugMessageType.DDATA or SparkplugMessageType.DDEATH or SparkplugMessageType.DCMD => $"spBv1.0/{group ?? "+"}/{commandType}/#",
				_ => throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null),
			};
		}
	}
}
