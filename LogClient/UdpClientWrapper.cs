using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogClient
{
    public class UdpClientWrapper
    {
        /// <summary>
        /// Port number.
        /// </summary>
        private readonly int _port;

        public UdpClientWrapper(int port = 9999)
        {
            this._port = port;
        }

        public IObservable<byte[]> ReceiveData()
        {
            return Observable.Create<byte[]>(async (observer, token) =>
            {
                try
                {
                    using (var client = new UdpClient(_port))
                    {
                        token.Register(client.Close);

                        while (!token.IsCancellationRequested)
                        {
                            var result = await client.ReceiveAsync();
                            observer.OnNext(result.Buffer);
                        }
                    }
                    observer.OnCompleted();
                }
                catch (Exception err)
                {
                    observer.OnError(err);
                }
            });
        }

        private class UdpAsyncResult : IAsyncResult
        {
            public object AsyncState { get; set; }

            public WaitHandle AsyncWaitHandle { get; internal set; }

            public bool CompletedSynchronously { get; internal set; }

            public bool IsCompleted { get; internal set; }
        }
    }
}
