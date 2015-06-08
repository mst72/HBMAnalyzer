using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PostgresLogAnalyzer
{
    public class LogItem
    {
        public Guid Id { get; set; }

        public LogItem()
        {
            _value = new StringBuilder();
        }

        private StringBuilder _value;
        public string Value { get; set; }

        internal void AddValue(string m)
        {
            _value.AppendLine(m.Trim());
        }

        public void Done()
        {
            Value = _value.ToString();
        }

        public string Stamp { get; set; }
        public int Line { get; set; }
        public string Timezone { get; set; }
        public string Ip { get; set; }
        public string Login { get; set; }
        public string Id1 { get; set; }
        public string Id2 { get; set; }
        public string Kind { get; set; }
        public override string ToString()
        {
            return string.Format( "{0} at {1} ({2})", Kind, Line, Stamp );
        }

        public decimal DurationMs { get; set; }
        public string SubKind { get; set; }
    }
}
