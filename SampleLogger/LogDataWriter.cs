using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;

namespace SampleLogger
{
    internal class LogDataWriter
    {
        private readonly Logger _log = LogManager.GetLogger("udp");
        private readonly Logger _status = LogManager.GetLogger("status");

        public IObservable<Unit> ProduceLogMessages()
        {
            return Observable.Create<Unit>(async (obseserver, token) =>
            {
                _status.Info("created writter");
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                    WriteLog();
                    _status.Info("next package sent");
                }
                _status.Info("disposed writter");
            });
        }

        private void WriteLog()
        {
            _log.Debug("debug message");
            _log.Info("info message");
            _log.Warn("warn message");
            _log.Error("error message");
        }
    }
}
