using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HBMLogAnalyzer
{
    class QueriesDict
    {
        public Dictionary<string,string> dict = new Dictionary<string, string>(); 
        internal void Load(string p)
        {
            foreach (var item in Directory.EnumerateFiles(p, "*.pgsql"))
            {
                var sdata = string.Join(Environment.NewLine, File.ReadAllLines(item, Encoding.UTF8).Select(x => x.Trim()));
                var fname = Path.GetFileNameWithoutExtension(item);
                dict[fname] = sdata;
            }
        }

        internal string GetQuery(string value, int line)
        {
            foreach (var item in dict)
            {
                if (item.Value == value)
                {
                    return item.Key;
                }
            }
            return "";
        }
    }
}
