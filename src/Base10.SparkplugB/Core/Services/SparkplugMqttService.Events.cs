using System;
using System.Threading;
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

		// receive messages from MQTT
		protected async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
		{
			try
			{
				await OnMessageReceivedInternal(arg);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error processing message");
				Console.WriteLine(ex.ToString());
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
					if(state == null) {
						var invalidArgs = new InvalidMessageReceivedEventEventArgs(arg.ApplicationMessage.Topic.ToString(), arg.ApplicationMessage.Payload);
						await this.OnInvalidMessageReceived(invalidArgs);
						return;
					}
					var args = new NodeStateEventArgs(topic, state);
					await OnStateMessageReceived(args);
					break;
				default:
					throw new NotImplementedException();
			}
		}


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
		private async Task OnInvalidMessageReceived(InvalidMessageReceivedEventEventArgs args)
		{
			await _invalidMessageReceivedEvent.InvokeAsync(args);
		}


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
		private async Task OnStateMessageReceived(NodeStateEventArgs args)
		{
			await _stateMessageReceivedEvent.InvokeAsync(args);
		}
	}
}
