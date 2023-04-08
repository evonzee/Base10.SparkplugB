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
		protected async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
		{
			try
			{
				await OnMessageReceivedInternal(arg);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error processing message");
				var invalidArgs = new InvalidMessageReceivedEventEventArgs(arg.ApplicationMessage.Topic.ToString(), arg.ApplicationMessage.Payload);
				await this.OnInvalidMessageReceived(invalidArgs);
			}
		}

		private async Task OnMessageReceivedInternal(MqttApplicationMessageReceivedEventArgs arg)
		{
			arg.AutoAcknowledge = true;
			var topic = _topicParser.Parse(arg.ApplicationMessage.Topic);
			switch (topic.Command)
			{
				case Enums.CommandType.STATE:
					var state = _messageParser.ParseState(arg.ApplicationMessage.Payload);
					await FireEvent(arg, state, async (s) =>
					{
						var args = new NodeStateEventArgs(topic, s);
						await OnStateMessageReceived(args);
					});
					break;
				case Enums.CommandType.NBIRTH:
					var payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeBirthReceived(args);
					});
					break;
				case Enums.CommandType.NDATA:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeDataReceived(args);
					});
					break;
				case Enums.CommandType.NDEATH:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeDeathReceived(args);
					});
					break;
				case Enums.CommandType.NCMD:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnNodeCommandReceived(args);
					});
					break;
				case Enums.CommandType.DBIRTH:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceBirthReceived(args);
					});
					break;
				case Enums.CommandType.DDATA:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceDataReceived(args);
					});
					break;
				case Enums.CommandType.DDEATH:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceDeathReceived(args);
					});
					break;
				case Enums.CommandType.DCMD:
					payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async (p) =>
					{
						var args = new SparkplugEventArgs(topic, p);
						await OnDeviceCommandReceived(args);
					});
					break;
				default:
					throw new NotImplementedException();
			}
		}

		// function to fire the actual event unless the payload is null, in which case fire invalid
		private async Task FireEvent<T>(MqttApplicationMessageReceivedEventArgs arg, T? state, Func<T, Task> handler)
		{
			if (state != null)
			{
				await handler(state);
			}
			else
			{
				var invalidArgs = new InvalidMessageReceivedEventEventArgs(arg.ApplicationMessage.Topic.ToString(), arg.ApplicationMessage.Payload);
				await this.OnInvalidMessageReceived(invalidArgs);
			}
		}

		// Invalid message
		private readonly AsyncEvent<InvalidMessageReceivedEventEventArgs> _invalidMessageReceivedEvent = new();
		public event Func<InvalidMessageReceivedEventEventArgs, Task> InvalidMessageReceived
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
		protected virtual async Task OnInvalidMessageReceived(InvalidMessageReceivedEventEventArgs args)
		{
			await _invalidMessageReceivedEvent.InvokeAsync(args);
		}

		//  Node State
		private readonly AsyncEvent<NodeStateEventArgs> _stateMessageReceivedEvent = new();
		public event Func<NodeStateEventArgs, Task> StateMessageReceived
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
		protected virtual async Task OnStateMessageReceived(NodeStateEventArgs args)
		{
			await _stateMessageReceivedEvent.InvokeAsync(args);
		}

		// Node Birth
		private readonly AsyncEvent<SparkplugEventArgs> _nodeBirthReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeBirthReceived
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
		protected virtual async Task OnNodeBirthReceived(SparkplugEventArgs args)
		{
			await _nodeBirthReceivedEvent.InvokeAsync(args);
		}

		// Node Data
		private readonly AsyncEvent<SparkplugEventArgs> _nodeDataReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeDataReceived
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
		protected virtual async Task OnNodeDataReceived(SparkplugEventArgs args)
		{
			await _nodeDataReceivedEvent.InvokeAsync(args);
		}

		//Node Death
		private readonly AsyncEvent<SparkplugEventArgs> _nodeDeathReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeDeathReceived
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
		protected virtual async Task OnNodeDeathReceived(SparkplugEventArgs args)
		{
			await _nodeDeathReceivedEvent.InvokeAsync(args);
		}

		// Node Command
		private readonly AsyncEvent<SparkplugEventArgs> _nodeCommandReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> NodeCommandReceived
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
		protected virtual async Task OnNodeCommandReceived(SparkplugEventArgs args)
		{
			await _nodeCommandReceivedEvent.InvokeAsync(args);
		}

		// Device Birth
		private readonly AsyncEvent<SparkplugEventArgs> _deviceBirthReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceBirthReceived
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
		protected virtual async Task OnDeviceBirthReceived(SparkplugEventArgs args)
		{
			await _deviceBirthReceivedEvent.InvokeAsync(args);
		}

		// Device Data
		private readonly AsyncEvent<SparkplugEventArgs> _deviceDataReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceDataReceived
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
		protected virtual async Task OnDeviceDataReceived(SparkplugEventArgs args)
		{
			await _deviceDataReceivedEvent.InvokeAsync(args);
		}

		// Device Death
		private readonly AsyncEvent<SparkplugEventArgs> _deviceDeathReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceDeathReceived
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
		protected virtual async Task OnDeviceDeathReceived(SparkplugEventArgs args)
		{
			await _deviceDeathReceivedEvent.InvokeAsync(args);
		}

		// Device Command
		private readonly AsyncEvent<SparkplugEventArgs> _deviceCommandReceivedEvent = new();
		public event Func<SparkplugEventArgs, Task> DeviceCommandReceived
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
		protected virtual async Task OnDeviceCommandReceived(SparkplugEventArgs args)
		{
			await _deviceCommandReceivedEvent.InvokeAsync(args);
		}
	}
}
