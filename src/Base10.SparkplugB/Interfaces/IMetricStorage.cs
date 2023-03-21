using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base10.SparkplugB.Interfaces
{
    /// <summary>
    /// Provides a storage structure for metrics.  Instances classes implementing this interface are used
    /// for Node and Device metric storage, since each node requires its own database.
    /// 
    /// An instance of this storage class will be created for every Node encountered.  For Node implementations,
    /// that may just be the node itself.  For Application implementations, there may be many instances.
    /// 
    /// </summary>
    /// <seealso href="https://sparkplug.eclipse.org/specification/version/3.0/documents/sparkplug-specification-3.0.0.pdf">SparkplugB 3.0 TCK, search for tck-id-payloads-alias-uniqueness</seealso>
    public interface IMetricStorage<N,D> where N: IMetric where D: IDeviceMetric
    {
        /// <summary>
        /// Adds a new node metric to this storage instance.
        /// If a metric already exists with the same name, discard it and store the new metric with the same alias.
        /// </summary>
        /// <param name="name">The name of this metric.</param>
        /// <param name="type">The type of this metric.  See @Base10.SparkplugB.Protocol.DataType</param>
        /// <param name="value">The initial value for this metric</param>
        /// <param name="hasAlias">Should this metric have an alias generated. If false, no alias will be 
        /// included in *BIRTH messages, and all packets containing this tag will use the full name</param>
        /// <returns></returns>
        public Task<N> AddNodeMetric(string name, Base10.SparkplugB.Protocol.DataType type, object value, bool hasAlias = true);

        /// <summary>
        /// Adds this node metric to this storage instance.  Resets aliases, unless hasAlias is false.
        /// If a node metric already exists with the same name, discard it and store the new metric with the same alias.
        /// </summary>
        /// <param name="metric">The Metric to add.</param>
        /// <returns></returns>
        public Task<N> AddNodeMetric(N metric);

        /// <summary>
        /// Adds a new device metric to this storage instance.
        /// If a metric already exists with the same name, discard it and store the new metric with the same alias.
        /// </summary>
        /// <param name="name">The name of this metric.</param>
        /// <param name="type">The type of this metric.  See @Base10.SparkplugB.Protocol.DataType</param>
        /// <param name="value">The initial value for this metric</param>
        /// <param name="hasAlias">Should this metric have an alias generated. If false, no alias will be 
        /// included in *BIRTH messages, and all packets containing this tag will use the full name</param>
        /// <returns></returns>
        public Task<D> AddDeviceMetric(string deviceName, string name, Base10.SparkplugB.Protocol.DataType type, object value, bool hasAlias = true);

        /// <summary>
        /// Adds this device metric to this storage instance.  Resets aliases, unless hasAlias is false.
        /// If a device metric already exists with the same name, discard it and store the new metric with the same alias.
        /// </summary>
        /// <param name="metric">The Metric to add.</param>
        /// <returns></returns>
        public Task<D> AddDeviceMetric(D metric);

        /// <summary>
        /// Gets the alias for an existing node metric.  Throws InvalidMetricException if no metric exists by this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<int> GetAlias(string name); 

        /// <summary>
        /// Gets the alias for an existing device metric.  Throws InvalidMetricException if no metric exists by this name
        /// </summary>
        /// <param name="deviceName">The device name</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public Task<int> GetAlias(string deviceName, string name); 

        /// <summary>
        /// Gets the Metric instance using the alias.  Throws InvalidMetricException if no metric exists with this alias.
        /// 
        /// Note that this could be an IDeviceMetric as well as a plain IMetric, but Device metrics will be delivered with DDATA/DCMD 
        /// so it probably doesn't matter for consumers of this method.
        /// </summary>
        /// <param name="alias">The alias to look up</param>
        /// <returns>The Metric instance with this alias, if found</returns>
        public Task<IMetric> GetByAlias(int alias);

        /// <summary>
        /// Returns all known metrics in this storage instance
        /// </summary>
        /// <returns>An IEnumerable of the known metrics</returns>
        public Task<IEnumerable<IMetric>> GetAll();
    }
}