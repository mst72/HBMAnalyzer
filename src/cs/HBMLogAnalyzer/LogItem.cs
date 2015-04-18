using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HBMLogAnalyzer
{
    public enum LoggerTypes
    {
        NamedQuery,
        Undef,
        Loader,
        SQL,
        Transaction,
        Common
    }

    public enum Operations
    {
        Undef,
        Select,
        Insert,
        Delete,
        Update,
        Invalid
    }

    public enum Transactions
    {
        Undef,
        Invalid,
        Begin,
        Commit,
        Rollback,
        Enlist,
        CommitDone,
        Disposed
    }

    class LogItem
    {
        public LogItem()
        {
            Value = new StringBuilder();
        }

        public StringBuilder Value { get; set; }
        internal void AddValue(string m)
        {
            Value.AppendLine(m.Trim());
        }

        public string Stamp { get; set; }

        public string Thread { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public LoggerTypes LoggerType
        {
            get
            {
                switch (Logger)
                {
                    case "NHibernate.Loader.Custom.Sql.SQLCustomQuery":
                        return LoggerTypes.NamedQuery;
                    case "NHibernate.Loader.Loader":
                        return LoggerTypes.Loader;
                    case "NHibernate.SQL":
                        return LoggerTypes.SQL;
                    case "NHibernate.Transaction.AdoTransaction":
                        return LoggerTypes.Transaction;
                    case "Common":
                        return LoggerTypes.Common;
                    default:
                        return LoggerTypes.Undef;
                }

            }
        }

        public bool IsValid()
        {
            return true;
            return !Value.ToString().Contains("NHibernate");
        }

        public int Line { get; set; }
        public string QueryName { get; set; }
        public Operations Operation { get; set; }
        public string Delta { get; set; }

        private string _getDelta(string fromStamp, string toStamp)
        {
            if (string.IsNullOrEmpty(fromStamp))
            {
                return "";
            }
            if (string.IsNullOrEmpty(toStamp))
            {
                return "";
            }
            var dt = DateTime.ParseExact(fromStamp, "yyyy-MM-dd HH:mm:ss,fff", null);
            var dt2 = DateTime.ParseExact(toStamp, "yyyy-MM-dd HH:mm:ss,fff", null);
            var res = dt2.Subtract(dt);
            return string.Format("{0:0.000}", res.TotalSeconds);
        }

        internal void UpdateDelta(string stamp)
        {
            Delta = _getDelta(Stamp, stamp);
        }


        public Transactions Transaction { get; set; }

        internal bool CanDumpTransaction()
        {
            return new[] {Transactions.Begin, Transactions.Commit, Transactions.Rollback, Transactions.Invalid}.Contains(Transaction);
        }

        internal string GetDelta(LogItem first)
        {
            return _getDelta(first.Stamp, Stamp);
        }

        public LogItem LastNamedItem { get; set; }

        public string Body { get; set; }

        public bool Hide { get; set; }
    }
}
