using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Enums;
using Base10.SparkplugB.Core.Filters;
using Base10.SparkplugB.Interfaces;
using Base10.SparkplugB.Protocol;
using FluentAssertions;
using Moq;
using static Base10.SparkplugB.Protocol.Payload.Types;

namespace Base10.SparkplugB.Tests.Core.Filters
{
	public class DecodeKnownMetricsFilterTests
	{
		[Theory]
		[InlineData(SparkplugMessageType.STATE)]
		[InlineData(SparkplugMessageType.NBIRTH)]
		[InlineData(SparkplugMessageType.DBIRTH)]
		public async Task BirthAndStatusMessagesPassThrough(SparkplugMessageType type)
		{
			var metricsStore = new Mock<IMetricsStore>();
			var filter = new DecodeKnownMetricsFilter(metricsStore.Object);
			var topic = new SparkplugTopic(type, "group", "node", "device");
			var payload = new Payload();
			var result = await filter.FilterAsync(topic, payload);

			result.Should().BeSameAs(payload);
			metricsStore.VerifyNoOtherCalls();
		}

		[Theory]
		[InlineData(SparkplugMessageType.NCMD)]
		[InlineData(SparkplugMessageType.NDATA)]
		[InlineData(SparkplugMessageType.DCMD)]
		[InlineData(SparkplugMessageType.DDATA)]
		public async Task CommandsAndDataAreFiltered(SparkplugMessageType type)
		{
			var metricsStore = new Mock<IMetricsStore>();
			var filter = new DecodeKnownMetricsFilter(metricsStore.Object);
			var topic = new SparkplugTopic(type, "group", "node", "device");
			var payload = new Payload();
			var result = await filter.FilterAsync(topic, payload);

			result.Should().NotBeSameAs(payload);
		}

		[Fact]
		public async Task MetricsWithAliasAreDecoded()
		{
			var metricsStore = new Mock<IMetricsStore>();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", 1)).ReturnsAsync(new Metric(){Name = "metric1", Alias = 1}).Verifiable();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", 2)).ReturnsAsync(new Metric(){Name = "metric2", Alias = 2}).Verifiable();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", 3)).ReturnsAsync(new Metric(){Name = "metric3", Alias = 3}).Verifiable();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", 4)).ReturnsAsync(new Metric(){Name = "metric4", Alias = 4});
			var filter = new DecodeKnownMetricsFilter(metricsStore.Object);
			var topic = new SparkplugTopic(SparkplugMessageType.NCMD, "node", "group", "device");

			var payload = new Payload();
			payload.Metrics.Add(new Metric(){Alias = 1});
			payload.Metrics.Add(new Metric(){Alias = 2});
			payload.Metrics.Add(new Metric(){Alias = 3});
			var result = await filter.FilterAsync(topic, payload);

			result.Metrics[0].Name.Should().Be("metric1");
			result.Metrics[1].Name.Should().Be("metric2");
			result.Metrics[2].Name.Should().Be("metric3");
			metricsStore.Verify();
			metricsStore.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task UnknownAliasesRaiseEventsOnce()
		{
			var metricsStore = new Mock<IMetricsStore>();
			var filter = new DecodeKnownMetricsFilter(metricsStore.Object);
			var topic = new SparkplugTopic(SparkplugMessageType.NCMD, "node", "group", "device");

			var payload = new Payload();
			payload.Metrics.Add(new Metric(){Alias = 1});
			payload.Metrics.Add(new Metric(){Alias = 2});
			payload.Metrics.Add(new Metric(){Alias = 3});

			int eventRaised = 0;
			filter.InvalidMetricReceivedAsync += (args) => { eventRaised++; return Task.CompletedTask; };
			var result = await filter.FilterAsync(topic, payload);

			eventRaised.Should().Be(1);
		}

		[Fact]
		public async Task NamedMetricsAreRecognized()
		{
			var metricsStore = new Mock<IMetricsStore>();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", "metric1")).ReturnsAsync(new Metric(){Name = "metric1", Alias = 1}).Verifiable();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", "metric2")).ReturnsAsync(new Metric(){Name = "metric2", Alias = 2}).Verifiable();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", "metric3")).ReturnsAsync(new Metric(){Name = "metric3", Alias = 3}).Verifiable();
			metricsStore.Setup(x => x.GetMetricAsync("group", "node", "device", "metric4")).ReturnsAsync(new Metric(){Name = "metric4", Alias = 4});
			var filter = new DecodeKnownMetricsFilter(metricsStore.Object);
			var topic = new SparkplugTopic(SparkplugMessageType.NCMD, "node", "group", "device");

			var payload = new Payload();
			payload.Metrics.Add(new Metric(){Name = "metric1"});
			payload.Metrics.Add(new Metric(){Name = "metric2"});
			payload.Metrics.Add(new Metric(){Name = "metric3"});
			var result = await filter.FilterAsync(topic, payload);

			result.Metrics[0].Alias.Should().Be(0); // if we aren't using aliases, we don't need to set them
			result.Metrics[1].Alias.Should().Be(0);
			result.Metrics[2].Alias.Should().Be(0);
			metricsStore.Verify();
			metricsStore.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task NewMetricsRaiseEventsOnce()
		{
			var metricsStore = new Mock<IMetricsStore>();
			var filter = new DecodeKnownMetricsFilter(metricsStore.Object);
			var topic = new SparkplugTopic(SparkplugMessageType.NCMD, "node", "group", "device");

			var payload = new Payload();
			payload.Metrics.Add(new Metric(){Name = "metric1"});
			payload.Metrics.Add(new Metric(){Name = "metric2"});
			payload.Metrics.Add(new Metric(){Name = "metric3"});

			int eventRaised = 0;
			filter.InvalidMetricReceivedAsync += (args) => { eventRaised++; return Task.CompletedTask; };
			var result = await filter.FilterAsync(topic, payload);

			eventRaised.Should().Be(1);
		}
	}
}
