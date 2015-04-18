using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HBMLogAnalyzer
{
    internal class HbmMapping
    {
        public void Process(string systemPath, string queriesPath)
        {
            foreach (var file in Directory.EnumerateFiles(queriesPath, "*" + HbmProcessor.Ext, SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.EnumerateFiles(systemPath, "*.hbm.xml", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file);
                if (fileName == null)
                    continue;
                processFile(file, queriesPath);
            }
        }

        private static void processFile(string fileName, string queriesPath)
        {
            Console.WriteLine("Process File {0}", fileName);
            var p = new HbmProcessor(fileName, queriesPath);
            p.ProcessIt();
        }

    }
}
