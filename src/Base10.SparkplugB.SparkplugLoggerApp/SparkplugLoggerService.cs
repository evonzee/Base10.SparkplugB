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
		private readonly SparkplugListener _listener;
		private readonly ILogger<SparkplugLoggerService> _logger;

		public SparkplugLoggerService(SparkplugListener listener, ILogger<SparkplugLoggerService> logger)
		{
			_listener = listener;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_listener.NodeBirthReceived += this.LogSparkplugEvent;
			_listener.NodeDeathReceived += this.LogSparkplugEvent;
			_listener.NodeDataReceived += this.LogSparkplugEvent;
			_listener.NodeCommandReceived += this.LogSparkplugEvent;
			_listener.DeviceBirthReceived += this.LogSparkplugEvent;
			_listener.DeviceDeathReceived += this.LogSparkplugEvent;
			_listener.DeviceDataReceived += this.LogSparkplugEvent;
			_listener.DeviceCommandReceived += this.LogSparkplugEvent;
			_listener.StateMessageReceived += this.LogStatusMessage;
			_listener.InvalidMessageReceived += this.LogInvalidMessage;

			await _listener.Connect();

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
