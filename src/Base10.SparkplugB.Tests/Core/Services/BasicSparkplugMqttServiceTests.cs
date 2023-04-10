using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class BasicSparkplugMqttServiceTests
	{
		[Fact]
		public void CommandSequenceNumberIsThreadSafe()
		{
			var app = new Mock<ExposedSparkplugMqttService>()
			{
				CallBase = true
			}.Object;
			var tasks = new List<Task>();
			for (int i = 0; i < 100; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					for (int j = 0; j < 100; j++)
					{
						app.NextCommandSequence();
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());
			app.NextCommandSequence().Should().Be(10000 % 256);
		}

		[Fact]
		public void BirthSequenceNumberIsThreadSafe()
		{
			var app = new Mock<ExposedSparkplugMqttService>()
			{
				CallBase = true
			}.Object;

			app.NextBirthSequence();
			app.CurrentBirthSequence().Should().Be(0);

			var tasks = new List<Task>();
			for (int i = 0; i < 100; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					for (int j = 0; j < 100; j++)
					{
						app.NextBirthSequence();
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());
			app.CurrentBirthSequence().Should().Be(10000 % 256);
		}
	}
}
