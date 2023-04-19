using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Events;
using Base10.SparkplugB.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Base10.SparkplugB.ApplicationDemo
{
	public class SparkplugExampleService : IHostedService
	{
		private readonly ILogger<SparkplugExampleService> _logger;
		private readonly SparkplugListener _app;

		public SparkplugExampleService(SparkplugListener app, ILogger<SparkplugExampleService> logger)
		{
			_logger = logger;
			_app = app;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_app.NodeBirthReceivedAsync += this.LogSparkplugEvent;
			_app.NodeDeathReceivedAsync += this.LogSparkplugEvent;
			_app.NodeDataReceivedAsync += this.LogSparkplugEvent;
			_app.NodeCommandReceivedAsync += this.LogSparkplugEvent;
			_app.DeviceBirthReceivedAsync += this.LogSparkplugEvent;
			_app.DeviceDeathReceivedAsync += this.LogSparkplugEvent;
			_app.DeviceDataReceivedAsync += this.LogSparkplugEvent;
			_app.DeviceCommandReceivedAsync += this.LogSparkplugEvent;
			_app.StateMessageReceivedAsync += this.LogStatusMessage;
			_app.InvalidMessageReceivedAsync += this.LogInvalidMessage;

			await _app.ConnectAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Sparkplug Example Service...");
			await _app.DisconnectAsync();
			_logger.LogInformation("Service stopped.");
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
