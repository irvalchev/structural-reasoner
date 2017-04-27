using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Logging
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public class LogItem
    {
        internal LogItem(LogType type, string message)
        {
            this.Type = type;
            this.Message = message;
            this.Timestamp = DateTime.Now;
        }

        public LogType Type { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1,-8} {2}", this.Timestamp, this.TypeToString(this.Type).ToUpper(), this.Message);
        }

        private string TypeToString(LogType type)
        {
            switch (type)
            {
                case LogType.Info:
                    return "Info";
                case LogType.Warning:
                    return "Warning";
                case LogType.Error:
                    return "Error";
                default:
                    return "";
            }
        }
    }
}
