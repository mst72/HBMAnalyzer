using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HBMLogAnalyzer
{
    public class LogParser
    {
        private static Regex logMark = new Regex(@"^(?<stamp>\d{4}\-\d{2}\-\d{2}\s+\d{2}\:\d{2}\:\d{2}(\,\d+)?)\s+\[(?<thread>\d+)\]\s+(?<level>DEBUG|INFO|ERROR)\s+(?<logger>[\w\d\.]+)\s+\-\s+(?<chunk>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Match CheckLogMark(string testLine)
        {
            return logMark.Match(testLine);
        }

        internal void Load(string p)
        {
            // 2015-01-12 22:40:08,579 [9] DEBUG NHibernate.Loader.Loader - processing result set
            int line = 1;
            foreach (var item in File.ReadAllLines(p))
            {
                var m = CheckLogMark(item);
                if (m.Success)
                {
                    InitCurrentItem(line, m.Groups["stamp"].ToString(), m.Groups["thread"].ToString(),
                        m.Groups["level"].ToString(), m.Groups["logger"].ToString(), m.Groups["chunk"].ToString());
                    line++;
                    continue;
                }
                AddValueToCurrentItem(line, item);
                line++;
            }
        }

        private LogItem currentItem = null;
        private LogItem lastNamedItem = null;
        private List<LogItem> items = new List<LogItem>(); 
        private void AddValueToCurrentItem(int line, string m)
        {
            if (currentItem == null)
            {
                InitCurrentItem(line, "", "", "", "", m);
            }
            else
            {
                currentItem.AddValue(m);
            }
        }

        private void InitCurrentItem(int line, string stamp, string thread, string level, string logger, string chunk)
        {
            currentItem = new LogItem();
            currentItem.Line = line;
            currentItem.Stamp = stamp;
            currentItem.Thread = thread;
            currentItem.Level = level;
            currentItem.Logger = logger;
            switch (currentItem.LoggerType)
            {
                case LoggerTypes.SQL:
                    SubProcessSQL(currentItem, chunk);
                    break;
                case LoggerTypes.Transaction:
                    SubProcessTransaction(currentItem, chunk);
                    break;
                case LoggerTypes.NamedQuery:
                    lastNamedItem = currentItem;
                    break;
            }
            currentItem.AddValue(chunk);
            items.Add(currentItem);
        }

        private void SubProcessTransaction(LogItem item, string chunk)
        {
            item.Transaction = LogParserHelper.RecognizeTransaction(chunk);
        }

        private void SubProcessSQL(LogItem item, string chunk)
        {
            item.Operation = LogParserHelper.RecognizeOperation(chunk);
            item.LastNamedItem = lastNamedItem;
            lastNamedItem = null;
        }



        internal void Validate()
        {
            foreach (var item in items)
            {
                if (!item.IsValid())
                {
                    Console.WriteLine(item.Value);
                    Console.WriteLine("---");
                }
            }
        }

        internal void Classify(QueriesDict dict)
        {
            foreach (var item in items)
            {
                switch (item.LoggerType)
                {
                    case LoggerTypes.NamedQuery:
                        ClassifyNamedQuery(dict, item);
                        break;
                    case LoggerTypes.SQL:
                        ClassifySQL(dict, item);
                        break;
                }
            }
        }


        private static void ClassifyNamedQuery(QueriesDict dict, LogItem item)
        {
            item.Body = LogParserHelper.GetNamedQueryBody(item.Value.ToString());
            item.QueryName = dict.GetQuery(item.Body, item.Line);
        }

        private static void ClassifySQL(QueriesDict dict, LogItem item)
        {
            if (item.Operation == Operations.Invalid)
            {
                return;
            }
            //var sData = item.Value.ToString();
            if (item.LastNamedItem != null)
            {
                // possible something interesting
                //var sData2 = item.LastNamedItem.Body;
                //if (sData.StartsWith(sData2))
                //{
                item.QueryName = item.LastNamedItem.QueryName;
                item.LastNamedItem.Hide = true;
                //}
            }
            else
            {
                //item.QueryName = dict.GetQuery(sData, item.Line);
            }
        }


        private string GetPrefix(LogItem item)
        {
            var delta = item.Delta;
            if (string.IsNullOrEmpty(delta))
            {
                return string.Format("{0} At {1} ", item.Stamp, item.Line);
            }
            else
            {
                return string.Format("{0} At {1} ({2}s)", item.Stamp, item.Line, item.Delta);
            }
        }

        public void Dump(string fileName)
        {
            var ctr = 0;
            var sb = new StringBuilder();
            LogItem pred = null;
            LogItem first = null;
            LogItem last = null;
            // todo: refactoring
            foreach (var item in items)
            {
                switch (item.LoggerType)
                {
                    case LoggerTypes.NamedQuery:
                        if (!item.Hide)
                        {
                            pred = UpdateDelta(item, pred);
                        }
                        break;
                    case LoggerTypes.Transaction:
                        if (item.CanDumpTransaction())
                        {
                            pred = UpdateDelta(item, pred);
                        }
                        break;
                    case LoggerTypes.Common:
                        pred = UpdateDelta(item, pred);
                        break;
                    case LoggerTypes.SQL:
                        if (item.Operation != Operations.Invalid)
                        {
                            pred = UpdateDelta(item, pred);
                        }
                        break;
                }
                if (first == null)
                {
                    first = pred;
                }
                last = pred;
            }
            foreach (var item in items)
            {
                switch (item.LoggerType)
                {
                    case LoggerTypes.NamedQuery:
                        if (!item.Hide)
                        {
                            var name = item.QueryName;
                            if (string.IsNullOrEmpty(name))
                            {
                                name = "???";
                            }
                            sb.AppendFormat("{0} ({1}) NamedQuery {2}", GetPrefix(item), ++ctr, name).AppendLine();
                        }
                        break;
                    case LoggerTypes.Transaction:
                        if (item.CanDumpTransaction())
                        {
                            sb.AppendFormat("{0} Transaction {1}", GetPrefix(item), 
                                LogParserHelper.ConvertToString(item.Value))
                                .AppendLine();
                        }
                        break;
                    case LoggerTypes.Common:
                        sb.AppendFormat("{0} Marker ({1}): {2}", GetPrefix(item), item.Level, 
                            LogParserHelper.ConvertToString(item.Value)).AppendLine();
                        break;
                    case LoggerTypes.SQL:
                        if (item.Operation != Operations.Invalid)
                        {
                            if (string.IsNullOrEmpty(item.QueryName))
                            {
                                sb.AppendFormat("{0} ({1}) SQL {2}: {3}", GetPrefix(item), ++ctr, item.Operation, 
                                    LogParserHelper.ConvertToString(item.Value)).AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("{0} ({1}) Named Query {2}: {3}", GetPrefix(item), ++ctr, item.QueryName, 
                                    LogParserHelper.ConvertToString(item.Value)).AppendLine();
                            }
                        }
                        break;
                }
            }
            if (first != null && last != null)
            {
                sb.AppendFormat("Total time: {0}s", last.GetDelta(first)).AppendLine();
            }
            sb.AppendFormat("Queries: {0}", ctr).AppendLine();
            File.WriteAllText(fileName, sb.ToString());
        }

        private LogItem UpdateDelta(LogItem item, LogItem pred)
        {
            if (pred != null)
            {
                pred.UpdateDelta(item.Stamp);
            }
            return item;
        }
    }
}
