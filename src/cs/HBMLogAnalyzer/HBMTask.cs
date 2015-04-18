using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HBMLogAnalyzer
{
    [Serializable]
    public class HBMTasks
    {
        public string SystemPath { get; set; }
        public string QueriesPath { get; set; }
        public List<HBMTask> Items { get; set; }

        internal void AddItem(HBMTask subTask)
        {
            if (Items == null)
            {
                Items = new List<HBMTask>();
            }
            Items.Add(subTask);
        }
    }

    [Serializable]
    public class HBMTask
    {
        public string LogPath { get; set; }
        public List<string> Files { get; set; }

        internal void AddItem(string file)
        {
            if (Files == null)
            {
                Files = new List<string>();
            }
            Files.Add(file);
        }
    }

}
