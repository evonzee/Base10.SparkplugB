using System;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Events;
using Base10.SparkplugB.Core.Internal;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Internal;

namespace Base10.SparkplugB.Core.Services
{
	public partial class SparkplugMqttService
	{
		private readonly SparkplugTopicParser _topicParser = new();
		private readonly SparkplugMessageParser _messageParser = new();

		// receive messages from MQTT and catch exceptions to raise an invalid event
		protected virtual async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
		{
			try
			{
				await OnMessageReceivedAsyncInternal(arg).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error processing message");
				var invalidArgs = new InvalidMessageReceivedEventEventArgs(arg.ApplicationMessage.Topic.ToString(), arg.ApplicationMessage.Payload);
				await this.OnInvalidMessageReceivedAsync(invalidArgs).ConfigureAwait(false);
			}
		}

		private async Task OnMessageReceivedAsyncInternal(MqttApplicationMessageReceivedEventArgs arg)
		{
			arg.AutoAcknowledge = true;
			var topic = _topicParser.Parse(arg.ApplicationMessage.Topic);
			switch (topic.Command)
			{
				case Enums.CommandType.STATE:
					var state = _messageParser.ParseState(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, state, async (s) =>
					{
						var args = new NodeStateEventArgs(topic, s);
						await OnStateMessageReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.NBIRTH:
					var payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeBirthReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.NDATA:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeDataReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.NDEATH:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeDeathReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.NCMD:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeCommandReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.DBIRTH:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceBirthReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.DDATA:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceDataReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.DDEATH:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceDeathReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				case Enums.CommandType.DCMD:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEventAsync(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceCommandReceivedAsync(args).ConfigureAwait(false);
					}).ConfigureAwait(false);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		// function to fire the actual event unless the payload is null, in which case fire invalid
		private async Task FireEventAsync<T>(MqttApplicationMessageReceivedEventArgs arg, T? state, Func<T, Task> handler)
		{
			if (state != null)
			{
				await handler(state).ConfigureAwait(false);
			}
			else
			{
				var invalidArgs = new InvalidMessageReceivedEventEventArgs(arg.ApplicationMessage.Topic.ToString(), arg.ApplicationMessage.Payload);
				await this.OnInvalidMessageReceivedAsync(invalidArgs).ConfigureAwait(false);
			}
		}

		// Invalid message
		private readonly AsyncEvent<InvalidMessageReceivedEventEventArgs> _invalidMessageReceivedEvent = new();
		public event Func<InvalidMessageReceivedEventEventArgs, Task> InvalidMessageReceivedAsync
		{
			add
			{
				_invalidMessageReceivedEvent.AddHandler(value);
			}
			remove
			{
				_invalidMessageReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnInvalidMessageReceivedAsync(InvalidMessageReceivedEventEventArgs args)
		{
			await _invalidMessageReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		//  Node State
		private readonly AsyncEvent<NodeStateEventArgs> _stateMessageReceivedEvent = new();
		public event Func<NodeStateEventArgs, Task> StateMessageReceivedAsync
		{
			add
			{
				_stateMessageReceivedEvent.AddHandler(value);
			}
			remove
			{
				_stateMessageReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnStateMessageReceivedAsync(NodeStateEventArgs args)
		{
			await _stateMessageReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Node Birth
		private readonly AsyncEvent<SparkplugEventArgs> _nodeBirthReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeBirthReceivedAsync
		{
			add
			{
				_nodeBirthReceivedEvent.AddHandler(value);
			}
			remove
			{
				_nodeBirthReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnNodeBirthReceivedAsync(SparkplugEventArgs args)
		{
			await _nodeBirthReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Node Data
		private readonly AsyncEvent<SparkplugEventArgs> _nodeDataReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeDataReceivedAsync
		{
			add
			{
				_nodeDataReceivedEvent.AddHandler(value);
			}
			remove
			{
				_nodeDataReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnNodeDataReceivedAsync(SparkplugEventArgs args)
		{
			await _nodeDataReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		//Node Death
		private readonly AsyncEvent<SparkplugEventArgs> _nodeDeathReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeDeathReceivedAsync
		{
			add
			{
				_nodeDeathReceivedEvent.AddHandler(value);
			}
			remove
			{
				_nodeDeathReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnNodeDeathReceivedAsync(SparkplugEventArgs args)
		{
			await _nodeDeathReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Node Command
		private readonly AsyncEvent<SparkplugEventArgs> _nodeCommandReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeCommandReceivedAsync
		{
			add
			{
				_nodeCommandReceivedEvent.AddHandler(value);
			}
			remove
			{
				_nodeCommandReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnNodeCommandReceivedAsync(SparkplugEventArgs args)
		{
			await _nodeCommandReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Device Birth
		private readonly AsyncEvent<SparkplugEventArgs> _deviceBirthReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceBirthReceivedAsync
		{
			add
			{
				_deviceBirthReceivedEvent.AddHandler(value);
			}
			remove
			{
				_deviceBirthReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnDeviceBirthReceivedAsync(SparkplugEventArgs args)
		{
			await _deviceBirthReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Device Data
		private readonly AsyncEvent<SparkplugEventArgs> _deviceDataReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceDataReceivedAsync
		{
			add
			{
				_deviceDataReceivedEvent.AddHandler(value);
			}
			remove
			{
				_deviceDataReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnDeviceDataReceivedAsync(SparkplugEventArgs args)
		{
			await _deviceDataReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Device Death
		private readonly AsyncEvent<SparkplugEventArgs> _deviceDeathReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceDeathReceivedAsync
		{
			add
			{
				_deviceDeathReceivedEvent.AddHandler(value);
			}
			remove
			{
				_deviceDeathReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnDeviceDeathReceivedAsync(SparkplugEventArgs args)
		{
			await _deviceDeathReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}

		// Device Command
		private readonly AsyncEvent<SparkplugEventArgs> _deviceCommandReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceCommandReceivedAsync
		{
			add
			{
				_deviceCommandReceivedEvent.AddHandler(value);
			}
			remove
			{
				_deviceCommandReceivedEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnDeviceCommandReceivedAsync(SparkplugEventArgs args)
		{
			await _deviceCommandReceivedEvent.InvokeAsync(args).ConfigureAwait(false);
		}
	}
}
