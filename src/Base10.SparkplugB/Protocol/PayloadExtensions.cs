using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Base10.SparkplugB.Protocol
{
    public static class PayloadExtensions
    {
        public static byte[] ToByteArray(this Payload payload)
		{
			using var stream = new MemoryStream();
			payload.WriteTo(stream);
			return stream.ToArray();
		}
    }
}
