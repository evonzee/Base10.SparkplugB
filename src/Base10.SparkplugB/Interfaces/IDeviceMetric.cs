using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Interfaces
{
    public interface IDeviceMetric : IMetric
    {
        public string DeviceName { get; }
    }
}