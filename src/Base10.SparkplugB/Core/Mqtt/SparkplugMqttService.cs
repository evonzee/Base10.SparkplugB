using System;
using System.Threading;
using System.Threading.Tasks;
using Base10.SparkplugB.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Base10.SparkplugB.Core.Mqtt
{
	public abstract class SparkplugMqttService<N, D> where N:IMetric where D:IDeviceMetric
	{

		private readonly string _mqttServerUri;
		private readonly string _clientId;
		private readonly string _username;
		private readonly string _password;
		private readonly string _group;
		private readonly IMetricStorage<N,D> _metricStorage;
		private int _sequence = 0;
		private object _sequenceLock = new object();

		private IManagedMqttClient _mqttClient;

		public SparkplugMqttService(string mqttServerUri, string clientId, string username, string password, string group, IMetricStorage<N, D> metricStorage)
		{
			_mqttServerUri = mqttServerUri;
			_clientId = clientId;
			_username = username;
			_password = password;
			_group = group;
			_metricStorage = metricStorage;
		}

		protected async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var optionsBuilder = new MqttClientOptionsBuilder()
				.WithClientId(_clientId)
				.WithTcpServer(_mqttServerUri)
				.WithCredentials(_username, _password);

			optionsBuilder = ConfigureLastWill(optionsBuilder);

			var options = optionsBuilder.Build();

			var managedOptions = new ManagedMqttClientOptionsBuilder()
				.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
				.WithClientOptions(options)
				.Build();

			_mqttClient = new MqttFactory().CreateManagedMqttClient();

			// add handlers

			await _mqttClient.StartAsync(managedOptions);

			// subscribe to appropriate topics per configuration
			await Subscribe(_mqttClient);

			// send birth message
			await SendBirthSequence(_mqttClient);

			// hang out and wait for events or shutdown
			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
			}

			await _mqttClient.StopAsync();
		}

		protected int NextSequence() {
			lock(_sequenceLock) {
				_sequence++;
				if(_sequence > 255) _sequence = 0;
			}
			return _sequence;
		}

		protected abstract Task SendBirthSequence(IManagedMqttClient mqttClient);
		protected abstract Task Subscribe(IManagedMqttClient mqttClient);
		protected abstract MqttClientOptionsBuilder ConfigureLastWill(MqttClientOptionsBuilder optionsBuilder);
	}
}