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
	public class SparkplugExampleService : BackgroundService
	{
		private readonly ILogger<SparkplugExampleService> _logger;
		private readonly SparkplugListener _app;

		public SparkplugExampleService(SparkplugListener app, ILogger<SparkplugExampleService> logger)
		{
			_logger = logger;
			_app = app;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_app.NodeBirthReceived += this.LogSparkplugEvent;
			_app.NodeDeathReceived += this.LogSparkplugEvent;
			_app.NodeDataReceived += this.LogSparkplugEvent;
			_app.NodeCommandReceived += this.LogSparkplugEvent;
			_app.DeviceBirthReceived += this.LogSparkplugEvent;
			_app.DeviceDeathReceived += this.LogSparkplugEvent;
			_app.DeviceDataReceived += this.LogSparkplugEvent;
			_app.DeviceCommandReceived += this.LogSparkplugEvent;
			_app.StateMessageReceived += this.LogStatusMessage;
			_app.InvalidMessageReceived += this.LogInvalidMessage;

			await _app.Connect();

			await Task.Delay(-1, stoppingToken);
			await _app.Disconnect();
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
