using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Configuration
{
    public record SparkplugServiceOptions(
		string ServerHostname,
		int ServerPort,
		bool UseTls,
		string ClientId,
		string Username,
		string Password,
		string Group
	) {}
}
