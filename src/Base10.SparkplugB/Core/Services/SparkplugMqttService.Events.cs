using System;
using System.Threading;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
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
					await FireEvent(arg, state, async () =>
					{
						var args = new NodeStateEventArgs(topic, state);
						await OnStateMessageReceived(args);
					});
					break;
				case Enums.CommandType.NBIRTH:
					var payload = _messageParser.ParseSparkplug(arg.ApplicationMessage.Payload);
					await FireEvent(arg, payload, async () =>
					{
						var args = new SparkplugEventArgs(topic, payload);
						await OnNodeBirthReceived(args);
					});
					break;
				default:
					throw new NotImplementedException();
			}
		}

		// function to fire the actual event unless the payload is null, in which case fire invalid
		private async Task FireEvent<T>(MqttApplicationMessageReceivedEventArgs arg, T? state, Func<Task> handler)
		{
			if (state != null)
			{
				await handler();
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
	}
}
