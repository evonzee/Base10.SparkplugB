using System;
using System.Threading;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Base10.SparkplugB.Core.Services
{
	public abstract class SparkplugMqttService
	{
		private readonly IManagedMqttClient _mqttClient;
		private readonly string _mqttServerUri;
		private readonly string _clientId;
		private readonly string _username;
		private readonly string _password;
		private long _sequence = -1; // basically guarantee we won't overflow for the life of this program(mer)
		protected readonly string _group;

		public SparkplugMqttService(string mqttServerUri, string clientId, string username, string password, string group)
		{
			_mqttServerUri = mqttServerUri;
			_clientId = clientId;
			_username = username;
			_password = password;
			_group = group;
			_mqttClient = new MqttFactory().CreateManagedMqttClient();
		}

		protected async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var optionsBuilder = new MqttClientOptionsBuilder()
				.WithClientId(_clientId)
				.WithTcpServer(_mqttServerUri)
				.WithCredentials(_username, _password)
				.WithCleanSession() // [tck-id-principles-persistence-clean-session-50]
				.WithSessionExpiryInterval(0); // [tck-id-principles-persistence-clean-session-50]

			optionsBuilder = ConfigureLastWill(optionsBuilder);

			var options = optionsBuilder.Build();

			var managedOptions = new ManagedMqttClientOptionsBuilder()
				.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
				.WithClientOptions(options)
				.Build();

			// add handlers

			await _mqttClient.StartAsync(managedOptions);

			// subscribe to appropriate topics per configuration
			// subscribe happens before birth, per [tck-id-host-topic-phid-birth-required]
			await Subscribe(_mqttClient);

			// send birth message
			await SendBirthSequence(_mqttClient); // apps must satisfy [tck-id-components-ph-state]

			// hang out and wait for events or shutdown
			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
			}

			await _mqttClient.StopAsync(false); // always send the last will, if configured
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

		protected abstract Task SendBirthSequence(IManagedMqttClient mqttClient);
		protected abstract Task Subscribe(IManagedMqttClient mqttClient);
		protected abstract MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder optionsBuilder);
	}
}
