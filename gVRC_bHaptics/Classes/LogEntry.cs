using System;

namespace gVRC_bHaptics.Classes
{
    internal class LogEntry
    {
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
