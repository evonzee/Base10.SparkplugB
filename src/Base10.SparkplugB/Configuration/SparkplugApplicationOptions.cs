using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Configuration
{
	public record SparkplugApplicationOptions : SparkplugServiceOptions
	{
		public bool Promiscuous { get; init; } = false;
	}
}
