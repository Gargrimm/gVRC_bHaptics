using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Audio;

namespace gVRC_bHaptics.Classes
{
    //TODO
    public class LogProxy
    {
        ILog Logger;
        Func<bool> ConfigDelegate;

        public static LogProxy GetLogger(Type type, Func<bool> configDelegate)
        {
            return new LogProxy(LogManager.GetLogger(type), configDelegate);
        }

        public static LogProxy GetLogger(string name, Func<bool> configDelegate)
        {
            return new LogProxy(LogManager.GetLogger(name), configDelegate);
        }

        public LogProxy(ILog logManager, Func<bool> configDelegate)
        {
            Logger = logManager;
            ConfigDelegate = configDelegate;
        }

        public void Debug(string message)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                Logger.Debug(message);
        }

        public void Error(string message, Exception ex)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                Logger.Error(message, ex);
        }

        public void Error(Exception ex)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                Logger.Error(ex);
        }

        public void Warn(string message)
        {
            if ((ConfigDelegate?.Invoke()).GetValueOrDefault())
                Logger.Warn(message);
        }
    }
}
