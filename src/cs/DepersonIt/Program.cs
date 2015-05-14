using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DepersonIt
{
    class Program
    {
        static readonly Regex ReDelims = new Regex(@"([\s\:\.\,\;\[\]\(\)\=\-\>\<\'\|\""\*\{\}\%\+\#/])", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex ReNumOnly = new Regex(@"^([\d]+)$", RegexOptions.Compiled);
        static readonly HashSet<string> IgnoredWords = new HashSet<string>()
        {
            // nHibernate profiler
            "loader", "as", "is", "nhibernate", "debug", "and", "or", "id", "left", "join", "outer", "sql", "done", "from",
            "null", "on", "result", "info", "set", "where", "entity", "select", "insert",
            "proxy", "initializing", "end", "order", "by", "begin", "common", "commit", "committed", "before", "with", "function",
            "package", "readcommitted", "custom", "adotransaction", "transaction", "entitykey", "inner", "date", "hydrated", "objects", "rows",
            "total", "command", "enlist", "group", "abstractentitypersister", "persister", "of", "query", "mapping", "to", "suffix", "starting", 
            "sqlcustomquery", "sqlqueryreturnprocessor", "stamp", "entitycriteriainfoprovider", "then", "when", "now", "name", "like", "if",
            "else", "distinct", "case", "current", "values", "string", "nextval", "into", "inserting",
            "processing", "model", "type", "this_", "true", "false", "cast", "alias", "criteria", "criteriaquerytranslator",
            "impl", "datetime", "datareader", "hydrating", "object", "sessionimpl", "completion",
            "for", "returning", "timestamp", "in", "boolean", "between", "time", "code", "to_char", "asc", "desc",
            "criteriaimpl", "dehydrating", "idbtransaction",
            "subcriteria", "start", "disposed", "hh", "mi", "ss", "put", "returning", "row", "getcriteria", "searching", "loading", "load",
            // postgresql DB log addon
            "eest", "log", "@", "checkpoint", "starting", "restartpoint", "complete", "wrote", "buffers", "file", "s", "added", "removed", "recycled", "write", "sync", "files", "longest", "average", 
            "recovery", "restart", "point", "last", "completed", "detail", "at", "xlog", "was", "wal", "stream", "ssl",
            "error", "sslv3", "alert", "unexpected", "message", "fatal", "could", "not", "receive",  "data",
            "invalid", "record", "length",
            "streaming", "replication", "successfully", "connected",
            "primary", "statement", "canceling", "due", "user", "request",
            "duration", "ms",
            "value", "too", "long", "character", "varying",
            "count", "extract", "month", "year", "bool"
        };

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid command line. Please use:");
                Console.WriteLine(" {0} path_to_original_file", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("Press return to continue...");
                Console.ReadLine();
                return 1;
            }
            var fname = args[0];
            if (!File.Exists(fname))
            {
                Console.WriteLine( "File {0} is not found", fname);
                Console.WriteLine("Press return to continue...");
                Console.ReadLine();
                return -1;
            }
            var ext = Path.GetExtension(fname);
            var resExt = string.Format("{0}_res", ext);
            var statExt = string.Format("{0}_stat", ext);
            var resFname = Path.ChangeExtension(fname, resExt);
            var statFname = Path.ChangeExtension(fname, statExt);
            Console.WriteLine("Source file: {0}", fname);
            Console.WriteLine("Target file: {0}", resFname);
            Console.WriteLine("  Stat file: {0}", statFname);
            DoIt(fname, resFname, statFname);
            return 0;
        }

        private static void DoIt(string fname, string resFname, string statFname)
        {
            var data = File.ReadAllText(fname);
            var words = new Dictionary<string, int>();
            var map = new Dictionary<string, string>();
            var items = Prepare(data, words, map);
            var res = Encode(items, map);
            File.WriteAllText(resFname, res.ToString());
            File.WriteAllText(statFname,
                string.Join(Environment.NewLine,
                    words.OrderByDescending(x => x.Value)
                        .ThenBy(x => x.Key)
                        .Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
        }

        private static StringBuilder Encode(string[] items, Dictionary<string, string> map)
        {
            var res = new StringBuilder();
            foreach (var item in items)
            {
                var encoded = "";
                if (map.TryGetValue(item, out encoded))
                {
                    res.Append(encoded);
                }
                else
                {
                    res.Append(item);
                }
            }
            return res;
        }

        private static string[] Prepare(string data, Dictionary<string, int> words, Dictionary<string, string> map)
        {
            // collect words
            var items = ReDelims.Split(data);
            foreach (var item in items)
            {
                if (IsNonDictItem(item)) continue;
                var cnt = 0;
                if (words.TryGetValue(item, out cnt))
                {
                    words[item] = cnt + 1;
                }
                else
                {
                    words[item] = cnt;
                }
            }
            // depersonalize
            var ctr = 0;
            foreach (var item in words.OrderBy(x => x.Key))
            {
                map[item.Key] = string.Format("{0}_{1}", "val", ++ctr);
            }
            return items;
        }

        private static bool IsNonDictItem(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(item))
            {
                return true;
            }
            if (ReDelims.IsMatch(item))
            {
                return true;
            }
            if (ReNumOnly.IsMatch(item))
            {
                return true;
            }
            if (IgnoredWords.Contains(item.ToLower()))
            {
                return true;
            }
            return false;
        }
    }
}
