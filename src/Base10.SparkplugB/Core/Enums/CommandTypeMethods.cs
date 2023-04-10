using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Core.Enums
{
    public static class CommandTypeMethods
    {
        public static string GetSubscriptionPattern(this CommandType commandType, string? group = null)
		{
			return commandType switch
			{
				CommandType.STATE => $"spBv1.0/STATE/#",
				CommandType.NBIRTH or CommandType.NDATA or CommandType.NDEATH or CommandType.NCMD or CommandType.DBIRTH or CommandType.DDATA or CommandType.DDEATH or CommandType.DCMD => $"spBv1.0/{group ?? "+"}/{commandType}/#",
				_ => throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null),
			};
		}
    }
}
