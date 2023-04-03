using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Internal;
using MQTTnet.Client;
using MQTTnet.Internal;

namespace Base10.SparkplugB.Core.Services
{
	public partial class SparkplugMqttService
	{
		private readonly SparkplugParser _topicParser = new SparkplugParser();

		// receive messages from MQTT
		private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
		{
			var topic = _topicParser.Parse(arg.ApplicationMessage);
			throw new NotImplementedException();
			// parse sparkplug
			// create the args for the event
			// raise it
		}
	}
}
