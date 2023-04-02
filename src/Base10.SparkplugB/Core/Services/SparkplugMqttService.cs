using System;
using System.Threading;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Internal; // unfortunate.. would be nice to break out the async event stuff

namespace Base10.SparkplugB.Core.Services
{
	public abstract class SparkplugMqttService
	{
		protected readonly MqttClientOptionsBuilder _mqttOptionsBuilder;
		protected readonly IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();
		protected readonly string _group;
		private long _sequence = -1; // basically guarantee we won't overflow for the life of this program(mer)
		private long _bdSequence = -1;

		public SparkplugMqttService(string serverHostname, int serverPort, bool useTls, string clientId, string username, string password, string group)
		{
			_mqttOptionsBuilder = new MqttClientOptionsBuilder()
				.WithClientId(clientId)
				.WithTcpServer(serverHostname, serverPort)
				.WithTls(o => {
					o.UseTls = useTls;
				})
				.WithCredentials(username, password)
				.WithCleanSession() // [tck-id-principles-persistence-clean-session-50]
				.WithSessionExpiryInterval(0); // [tck-id-principles-persistence-clean-session-50]
			_group = group;
		}

		protected async Task StartAsync()
		{
			var options = ConfigureLastWill(_mqttOptionsBuilder).Build();

			var managedOptions = new ManagedMqttClientOptionsBuilder()
				.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
				.WithClientOptions(options)
				.Build();

			// add handlers
			_mqttClient.ConnectedAsync += OnConnected;
			_mqttClient.DisconnectedAsync += OnDisconnected;


			await this.OnBeforeStart().ConfigureAwait(false);
			await _mqttClient.StartAsync(managedOptions).ConfigureAwait(false);
			await this.OnStarted().ConfigureAwait(false);
		}

		protected async Task StopAsync(bool cleanStop = false)
		{
			await _mqttClient.StopAsync(cleanStop).ConfigureAwait(false);
		}

		protected virtual MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder optionsBuilder)
		{
			return optionsBuilder;
		}

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

		#region Events

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
		}


		#endregion
	}
}
