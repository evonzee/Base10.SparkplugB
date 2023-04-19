using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Base10.SparkplugB.SparkplugLoggerApp
{
	public class SparkplugLoggerService : BackgroundService
	{
		private readonly SparkplugLoggerListener _listener;
		private readonly ILogger<SparkplugLoggerService> _logger;

		public SparkplugLoggerService(SparkplugLoggerListener listener, ILogger<SparkplugLoggerService> logger)
		{
			_listener = listener;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_listener.NodeBirthReceivedAsync += this.LogSparkplugEvent;
			_listener.NodeDeathReceivedAsync += this.LogSparkplugEvent;
			_listener.NodeDataReceivedAsync += this.LogSparkplugEvent;
			_listener.NodeCommandReceivedAsync += this.LogSparkplugEvent;
			_listener.DeviceBirthReceivedAsync += this.LogSparkplugEvent;
			_listener.DeviceDeathReceivedAsync += this.LogSparkplugEvent;
			_listener.DeviceDataReceivedAsync += this.LogSparkplugEvent;
			_listener.DeviceCommandReceivedAsync += this.LogSparkplugEvent;
			_listener.StateMessageReceivedAsync += this.LogStatusMessage;
			_listener.InvalidMessageReceivedAsync += this.LogInvalidMessage;

			await _listener.ConnectAsync();

			await Task.Delay(-1, stoppingToken);
		}

		private Task LogInvalidMessage(InvalidMessageReceivedEventEventArgs arg)
		{
			_logger.LogInformation("Invalid message received on topic {Topic}: {Message}", arg.Topic, BitConverter.ToString(arg.Payload));
			return Task.CompletedTask;
		}

		private Task LogStatusMessage(NodeStateEventArgs arg)
		{
			_logger.LogInformation("Node Status for [{NodeName}] is {Status} at {Timestamp}", arg.Topic.Node, arg.State.Online ? "Online" : "Offline", arg.State.TimestampAsDateTime);
			return Task.CompletedTask;
		}

		private Task LogSparkplugEvent(SparkplugEventArgs arg)
		{
			_logger.LogInformation("{Command} for [{NodeName}.{DeviceId}]: {@Payload}", arg.Topic.Command, arg.Topic.Node, arg.Topic.DeviceId, arg.Payload);
			return Task.CompletedTask;
		}
	}
}
