using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HBMLogAnalyzer
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid command line. Pleas use:");
                Console.WriteLine(" {0} tasks.cfg", AppDomain.CurrentDomain.FriendlyName );
                Console.WriteLine("Press return to continue...");
                Console.ReadLine();
                return;
            }
            var tasksName = args[0];
            var serializer = new XmlSerializer(typeof(HBMTasks), new[] { typeof(HBMTask) });
            using (var stream = new StreamReader(tasksName))
            {
                var task = (HBMTasks)serializer.Deserialize(stream);
                var hbmMappnog = new HbmMapping();
                hbmMappnog.Process(task.SystemPath, task.QueriesPath);
                if (task.Items != null)
                {
                    foreach (var item in task.Items)
                    {
                        if (item.Files != null)
                        {
                            foreach (var file in item.Files)
                            {
                                ProcessLog(task.QueriesPath, item.LogPath, file);
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessLog(string queriesPath, string logPath, string logName)
        {
            var parser = new LogParser();
            parser.Load( Path.Combine(logPath, logName));
            parser.Validate();
            var dict = new QueriesDict();
            dict.Load(queriesPath);
            parser.Classify(dict);
            var ext = Path.GetExtension(logName);
            var pureName = Path.GetFileNameWithoutExtension(logName);
            var resName = pureName + "_decode" + ext;
            parser.Dump( Path.Combine(logPath, resName));
            //Console.ReadLine();
        }
    }

}
