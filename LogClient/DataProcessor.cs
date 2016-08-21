using System;
using System.Reactive.Linq;
using System.Text;

namespace LogClient
{
    public class DataProcessor 
    {
        private readonly IObservable<TraceData> _dataStream;

        public DataProcessor(int portNumber)
        {
            var client = new UdpClientWrapper(portNumber);

            _dataStream = client.ReceiveData()
                .Where(p => null != p)
                .Select(StringFromBytes)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(DataFromString)
                .Where(p => null != p)
                .Publish()
                .RefCount();
        }

        public IObservable<TraceData> TraceDataStream
        {
            get { return _dataStream; }
        }

        public IObservable<string> CategoryDataStream
        {
            get { return _dataStream.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase); }
        }

        private static string StringFromBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private static TraceData DataFromString(string asString)
        {
            try
            {
                string[] parts = asString.Split('~');
                return new TraceData(parts);
            }
            catch
            {
                return null;
            }
        }
    }
}
