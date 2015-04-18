using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HBMLogAnalyzer
{
    public class LogParserHelper
    {
        private static Regex logOperation = new Regex(@"^(?<operation>SELECT|INSERT|DELETE|UPDATE)(?<chunk>(\s+.*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Operations RecognizeOperation(string chunk)
        {
            var m = logOperation.Match(chunk);
            if (!m.Success)
            {
                return Operations.Invalid;
            }
            return (Operations)Enum.Parse(typeof(Operations), m.Groups["operation"].ToString().ToLower(), true);
        }

        public static Transactions RecognizeTransaction(string chunk)
        {
            var data = chunk.ToLower();
            switch (data)
            {
                case "begin (readcommitted)":
                    return Transactions.Begin;
                case "enlist command":
                    return Transactions.Enlist;
                case "start commit":
                    return Transactions.Commit;
                case "idbtransaction committed":
                    return Transactions.CommitDone;
                case "idbtransaction disposed.":
                    return Transactions.Disposed;
                default:
                    return Transactions.Invalid;
                //throw new Exception(string.Format("Unsupported: [{0}]", data));
            }
        }

        public static string GetNamedQueryBody(string value)
        {
            var m = Regex.Match(value, @"^starting processing of sql query \[(?<body>.*)\]\s+$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return m.Groups["body"].ToString();
            }
            return "";
        }

        public static string ConvertToString(StringBuilder str)
        {
            var sData = str.ToString();
            if (sData.Length > 50)
            {
                sData = sData.Substring(0, 50) + "...";
            }
            sData = sData.Replace('\r', ' ').Replace('\n', ' ');
            return sData;
        }

    }
}
