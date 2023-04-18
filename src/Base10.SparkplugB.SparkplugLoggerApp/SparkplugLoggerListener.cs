using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Configuration;
using Base10.SparkplugB.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;

namespace Base10.SparkplugB.SparkplugLoggerApp
{
	public class SparkplugLoggerListener : SparkplugMqttService
	{
		public SparkplugLoggerListener(IOptions<SparkplugServiceOptions> options, ILogger<SparkplugLoggerListener> logger) : base(options.Value, null, logger)
		{
			this.Connected += this.OnConnected;
		}

		private async Task OnConnected(EventArgs arg)
		{
			await this._mqttClient.SubscribeAsync("spBv1.0/#");
			_logger?.LogInformation("Subscribed to all topics");
		}
	}
}
