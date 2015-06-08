using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PostgresLogAnalyzer
{
    public class LogParser
    {
        /*
         2015-04-09 16:00:03 EEST:                    :     @   :[4093] : [25293-1]: LOG:  
         2015-04-09 16:23:21 EEST:                    :     @   :[18582]: [2-1]: FATAL:
         2015-04-09 16:24:54 EEST:192.168.10.16(54238):TaMo@TaMo:[43856]: [1-1]: ERROR:
         */
        private static Regex logMark = new Regex(
                   @"^(?<stamp>\d{4}\-\d{2}\-\d{2}\s+\d{2}\:\d{2}\:\d{2})\s+(?<timezone>EEST)\:" +
                   @"\s*(?<ip>(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\(\d+\))?)\s*\:" + // 
                   @"\s*(?<login>[^:]*@[^:]*)\s*\:" +
                   @"\s*\[(?<id1>[\d]+)\]\s*\:" +
                   @"\s*\[(?<id2>[\d]+\-[\d]+)\]\s*\:" +
                   @"\s*(?<kind>LOG|DETAIL|ERROR|FATAL|STATEMENT)\s*\:" +
                   @"(?<chunk>.*)" +
                   @"$", 
                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Match CheckLogMark(string testLine)
        {
            return logMark.Match(testLine);
        }

        internal int Load(string p)
        {
            // 2015-01-12 22:40:08,579 [9] DEBUG NHibernate.Loader.Loader - processing result set
            int line = 0;
            foreach (var item in File.ReadAllLines(p))
            {
                var m = CheckLogMark(item);
                if (m.Success)
                {
                    InitCurrentItem(++line, m);
                    continue;
                }
                AddValueToCurrentItem(++line, item);
            }
            DoneItem();
            return line;
        }

        private LogItem currentItem = null;
        private LogItem lastNamedItem = null;
        private List<LogItem> items = new List<LogItem>();
        public List<LogItem> Items { get { return items; }} 
        private void AddValueToCurrentItem(int line, string m)
        {
            if (currentItem == null)
            {
                throw new Exception (string.Format( "unexpected item: [{0}]", m));
            }
            else
            {
                currentItem.AddValue(m);
            }
        }

        private void InitCurrentItem(int line, Match m)
        {
            DoneItem();
            currentItem = new LogItem();
            currentItem.Id = Guid.NewGuid();
            currentItem.Line = line;
            currentItem.Stamp = m.Groups["stamp"].ToString();
            currentItem.Timezone = m.Groups["timezone"].ToString();
            currentItem.Ip = m.Groups["ip"].ToString();
            currentItem.Login = m.Groups["login"].ToString().Trim();
            currentItem.Id1 = m.Groups["id1"].ToString();
            currentItem.Id2 = m.Groups["id2"].ToString();
            currentItem.Kind = m.Groups["kind"].ToString();
            currentItem.AddValue(m.Groups["chunk"].ToString());
            items.Add(currentItem);
        }

        private void DoneItem()
        {
            if (currentItem == null)
            {
                return;
            }
            currentItem.Done();
            switch (currentItem.Kind.ToLower())
            {
                case "log":
                    processLog(currentItem);
                    break;
            }
        }
        private static Regex logMark2 = new Regex(
                   @"^\s*duration:\s*(?<duration_val>[\d\.]+)\s+(?<duration_suffix>ms)" +
                   @"\s+statement:\s*(?<statement>.*)\s*" + // 
                   @"$",
                   RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public Match CheckLogMark2(string testLine)
        {
            return logMark2.Match(testLine);
        }

        private HashSet<string> starts = new HashSet<string>()
        {
            "restart", "recover", "invalid", "streaming", "temporary", "process"
        }; 
        //   duration: 1747.419 ms  statement: 
        private void processLog(LogItem currentItem)
        {
            foreach (var item in starts)
            {
                if (currentItem.Value.StartsWith(item))
                {
                    currentItem.SubKind = item;
                    return;
                }
            }
            var m = CheckLogMark2(currentItem.Value);
            if (!m.Success)
            {
                currentItem.SubKind = "undef";
                return;
            }
            currentItem.SubKind = "query";
            System.Globalization.CultureInfo ci =
                      System.Globalization.CultureInfo.InstalledUICulture;
            var _ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            _ni.NumberDecimalSeparator = ".";

            // convert to decimal
            currentItem.DurationMs = Convert.ToDecimal(m.Groups["duration_val"].ToString(), _ni);
            var suffix = m.Groups["duration_suffix"].ToString().ToLower();
            switch (suffix)
            {
                case "ms":
                    break;
                default:
                    throw new Exception(string.Format( "Unsupported suffix {0}", suffix) );
            }
            currentItem.Value = m.Groups["statement"].ToString();
        }
    }
}
