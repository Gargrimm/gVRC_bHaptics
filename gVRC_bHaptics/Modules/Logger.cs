using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using gVRC_bHaptics.Classes;

namespace gVRC_bHaptics.Modules
{
    internal class Logger
    {
        private object _lock = new object();
        public ObservableCollection<LogEntry> LogEntries = new ObservableCollection<LogEntry>();

        public Dispatcher Dispatcher;


        public void Log(Exception ex)
        {
            Common.InvokeIfNecessary(()=> DoLog(ex));
        }

        public void Log(string message, bool isError = false)
        {
            Common.InvokeIfNecessary(() => DoLog(message, isError));
        }

        private void DoLog(Exception ex)
        {
            lock (_lock)
            {

                LogEntries.Add(new LogEntry
                {
                    Date = DateTime.Now,
                    Message = ex.Message,
                    IsError = true,
                });
                LogEntries.Add(new LogEntry
                {
                    Date = DateTime.Now,
                    Message = ex.StackTrace,
                    IsError = true,
                });
            }

            if (ex.InnerException != null)
            {
                Log(ex.InnerException);
            }
        }

        private void DoLog(string message, bool isError = false)
        {

            lock (_lock)
            {
                LogEntries.Add(new LogEntry
                {
                    Date = DateTime.Now,
                    Message = message,
                    IsError = isError,
                });
            }
        }

    }
}
