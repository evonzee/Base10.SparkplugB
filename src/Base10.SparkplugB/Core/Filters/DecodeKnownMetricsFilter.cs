using System;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Enums;
using Base10.SparkplugB.Core.Events;
using Base10.SparkplugB.Interfaces;
using Base10.SparkplugB.Protocol;
using MQTTnet.Internal;

namespace Base10.SparkplugB.Core.Filters
{
	public class DecodeKnownMetricsFilter : IFilter
	{
		private readonly IMetricsStore _metricsStore;

		public DecodeKnownMetricsFilter(IMetricsStore metricsStore)
		{
			_metricsStore = metricsStore;
		}

		public async Task<Payload> FilterAsync(SparkplugTopic topic, Payload payload)
		{
			return topic.Command switch
			{
				SparkplugMessageType.NCMD or SparkplugMessageType.NDATA or SparkplugMessageType.DCMD or SparkplugMessageType.DDATA => await DecodeAsync(topic, payload).ConfigureAwait(false),
				_ => payload,
			};
		}

		private async Task<Payload> DecodeAsync(SparkplugTopic topic, Payload payload)
		{
			int badMetricCount = 0;

			foreach (var metric in payload.Metrics) {
				if (metric.Alias != 0) {
					var knownMetric = await _metricsStore.GetMetricAsync(topic.Group ?? "", topic.Node, topic.DeviceId, metric.Alias).ConfigureAwait(false);
					if (knownMetric != null) {
						metric.Name = knownMetric.Name;
					} else {
						metric.Name = $"Unknown {metric.Alias}";
						badMetricCount++;
					}
				}
				else
				{
					var knownMetric = await _metricsStore.GetMetricAsync(topic.Group ?? "", topic.Node, topic.DeviceId, metric.Name).ConfigureAwait(false);
					if (knownMetric == null) {
						badMetricCount++;
					}
				}
			}

			if(badMetricCount > 0) {
				await this.OnInvalidMetricReceivedAsync(new SparkplugEventArgs(topic, payload)).ConfigureAwait(false);
			}

			return payload;
		}

		private readonly AsyncEvent<SparkplugEventArgs> _invalidMetricEvent = new();
		public event Func<SparkplugEventArgs, Task> InvalidMetricReceivedAsync
		{
			add
			{
				_invalidMetricEvent.AddHandler(value);
			}
			remove
			{
				_invalidMetricEvent.RemoveHandler(value);
			}
		}
		protected virtual async Task OnInvalidMetricReceivedAsync(SparkplugEventArgs args)
		{
			await _invalidMetricEvent.InvokeAsync(args).ConfigureAwait(false);
		}
	}
}
