using System.Threading.Tasks;
using Base10.SparkplugB.Core.Data;
using Base10.SparkplugB.Protocol;

namespace Base10.SparkplugB.Core.Filters
{
    public interface IFilter
    {
        Task<Payload> FilterAsync(SparkplugTopic topic, Payload payload);
    }
}
