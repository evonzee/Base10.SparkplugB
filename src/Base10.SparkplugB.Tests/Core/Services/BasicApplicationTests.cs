using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Core.Services;
using Moq;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Services
{
	public class BasicApplicationTests
	{
		[Fact]
		public void CommandSequenceNumberIsThreadSafe()
		{
			var app = new Mock<ExposedApplication>()
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
			Assert.Equal(10000 % 256, app.NextCommandSequence());
		}

		[Fact]
		public void BirthSequenceNumberIsThreadSafe()
		{
			var app = new Mock<ExposedApplication>()
			{
				CallBase = true
			}.Object;

			app.NextBirthSequence();
			Assert.Equal(0, app.CurrentBirthSequence());

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
			Assert.Equal(10000 % 256, app.CurrentBirthSequence());
		}
	}
}
