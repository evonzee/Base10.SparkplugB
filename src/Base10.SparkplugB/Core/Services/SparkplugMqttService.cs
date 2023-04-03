using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal; // unfortunate.. would be nice to break out the async event stuff

namespace Base10.SparkplugB.Core.Services
{
	public abstract partial class SparkplugMqttService
	{
		protected readonly MqttClientOptionsBuilder _mqttOptionsBuilder;
		protected readonly IMqttClient _mqttClient = new MqttFactory().CreateMqttClient();
		protected readonly string _group;
		private long _sequence = -1; // basically guarantee we won't overflow for the life of this program(mer)
		private long _bdSequence = -1;
		private bool _shouldReconnect = false;

		public SparkplugMqttService(string serverHostname, int serverPort, bool useTls, string clientId, string username, string password, string group)
		{
			_mqttOptionsBuilder = new MqttClientOptionsBuilder()
				.WithClientId(clientId)
				.WithTcpServer(serverHostname, serverPort)
				.WithTls(o =>
				{
					o.UseTls = useTls;
				})
				.WithCredentials(username, password)
				.WithCleanSession() // [tck-id-principles-persistence-clean-session-50]
				.WithSessionExpiryInterval(0); // [tck-id-principles-persistence-clean-session-50]
			_group = group;
		}

		public async Task Connect()
		{
			this.NextBirthSequence(); // tie this to connect only and not birth due to [tck-id-payloads-nbirth-bdseq-repeat].  Could move into Edge node ConfigureLastWill implementation?
			var options = ConfigureLastWill(_mqttOptionsBuilder).Build();

			// add handlers
			_mqttClient.ConnectedAsync += OnConnected;
			_mqttClient.DisconnectedAsync += OnDisconnected;
			_mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

			_shouldReconnect = true;

			await this.OnBeforeStart().ConfigureAwait(false);
			await _mqttClient.ConnectAsync(options);
			await this.OnStarted().ConfigureAwait(false);
		}

		protected async Task Disconnect()
		{
			_shouldReconnect = false;
			// TODO: need a disconnecting event so we can publish death messages
			// per [tck-id-host-topic-phid-death-payload-disconnect-clean] and [tck-id-host-topic-phid-death-payload-disconnect-with-no-disconnect-packet]
			var options = new MqttFactory().CreateClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectReason.ServerShuttingDown).Build();
			await _mqttClient.DisconnectAsync(options).ConfigureAwait(false);
		}

		protected virtual MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder optionsBuilder)
		{
			return optionsBuilder;
		}

		#region Accessors for sequence numbers

		protected int NextCommandSequence()
		{
			Interlocked.Increment(ref _sequence);
			return (int)(_sequence % 256);
		}
		protected void ResetCommandSequence()
		{
			Interlocked.Exchange(ref _sequence, -1);
		}
		protected int NextBirthSequence()
		{
			Interlocked.Increment(ref _bdSequence);
			return CurrentBirthSequence();
		}
		public int CurrentBirthSequence()
		{
			return (int)(_bdSequence % 256);
		}

		#endregion

		#region MQTT lifecycle Events

		private readonly AsyncEvent<EventArgs> _beforeStartEvent = new AsyncEvent<EventArgs>();
		protected event Func<EventArgs, Task> BeforeStart
		{
			add
			{
				_beforeStartEvent.AddHandler(value);
			}
			remove
			{
				_beforeStartEvent.RemoveHandler(value);
			}
		}
		private async Task OnBeforeStart()
		{
			await _beforeStartEvent.InvokeAsync(new EventArgs());
		}

		private readonly AsyncEvent<EventArgs> _startedEvent = new AsyncEvent<EventArgs>();
		protected event Func<EventArgs, Task> Started
		{
			add
			{
				_startedEvent.AddHandler(value);
			}
			remove
			{
				_startedEvent.RemoveHandler(value);
			}
		}
		private async Task OnStarted()
		{
			await _beforeStartEvent.InvokeAsync(new EventArgs());
		}

		private readonly AsyncEvent<EventArgs> _connectedEvent = new AsyncEvent<EventArgs>();
		protected event Func<EventArgs, Task> Connected
		{
			add
			{
				_connectedEvent.AddHandler(value);
			}
			remove
			{
				_connectedEvent.RemoveHandler(value);
			}
		}
		private async Task OnConnected(MqttClientConnectedEventArgs arg)
		{
			await _connectedEvent.InvokeAsync(arg);
		}

		private readonly AsyncEvent<EventArgs> _disconnectedEvent = new AsyncEvent<EventArgs>();
		protected event Func<EventArgs, Task> Disconnected
		{
			add
			{
				_disconnectedEvent.AddHandler(value);
			}
			remove
			{
				_disconnectedEvent.RemoveHandler(value);
			}
		}
		private async Task OnDisconnected(MqttClientDisconnectedEventArgs arg)
		{
			await _disconnectedEvent.InvokeAsync(arg);
			if (_shouldReconnect)
			{
				await this.Connect();
			}
		}

		#endregion
	}
}
