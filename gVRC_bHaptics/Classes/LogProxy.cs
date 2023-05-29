using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.Classes
{
    //TODO
    public class LogProxy
    {
        ILog LogManager;
        Func<bool> ConfigDelegate;

        public LogProxy(ILog logManager, Func<bool> configDelegate)
        {
            LogManager = logManager;
            ConfigDelegate = configDelegate;
        }

        public void Debug(string message)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                LogManager.Debug(message);
        }

        public void Error(string message, Exception ex)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                LogManager.Error(message, ex);
        }

        public void Error(Exception ex)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                LogManager.Error(ex);
        }

    }
}
