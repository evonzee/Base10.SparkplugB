using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Core.Data
{
	public record NodeState(bool Online, long Timestamp)
	{
		public DateTime TimestampAsDateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
	}
}
