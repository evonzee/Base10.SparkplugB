using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;

namespace Base10.SparkplugB.ApplicationDemo
{
	public class SparkplugListener : SparkplugApplication
	{
		public SparkplugListener(IOptions<SparkplugServiceOptions> options, ILogger<SparkplugListener> logger) : base(options.Value, null, logger)
		{
		}
	}
}
