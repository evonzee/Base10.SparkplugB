using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Configuration
{
	public record SparkplugServiceOptions
	{

		public string ServerHostname { get; init; } = "";
		public int ServerPort { get; init; } = 1883;
		public bool UseTls { get; init; } = false;
		public string ClientId { get; init; } = "UnnamedClient";
		public string Username { get; init; } = "";
		public string Password { get; init; } = "";
		public string Group { get; init; } = "UnnamedGroup";
	}
}
