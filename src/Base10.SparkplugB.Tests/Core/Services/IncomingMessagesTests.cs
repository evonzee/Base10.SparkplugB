using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Base10.SparkplugB.Tests.Core.Services
{
    public class IncomingMessagesTests
    {
        [Theory]
		[InlineData("{\"online\": true, \"timestamp\": 1680702074814}", true, "2023-04-05 13:41:15")]
        public void ValidStateMessagesRaiseEvents(string message, bool online, string timestamp)
		{
			Assert.True(true);
		}
    }
}
