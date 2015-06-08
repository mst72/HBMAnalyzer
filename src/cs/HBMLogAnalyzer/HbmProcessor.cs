using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace HBMLogAnalyzer
{
    /// <summary>
    /// HBM Mapping processor (Read XML, Export Query)
    /// </summary>
    class HbmProcessor
    {
        private readonly string fileName;
        public static string Ext = ".pgsql";
        private string outPath;
        public HbmProcessor(string fileName, string outPath)
        {
            this.fileName = fileName;
            this.outPath = outPath;
        }

        public void ProcessIt()
        {
            var x = new XmlDocument();
            x.Load(fileName);
            processNode(x, "");
        }

        private void processNode(XmlNode x, string pad)
        {
            switch (x.Name)
            {
                case "sql-query":
                    ProcessSqlQuery(x);
                    break;
                case "return":
                    break;
                default:
                    break;
            }

            if (x is XmlCDataSection)
            {
                ProcessCData(x);
            }
            foreach (var _item in x.ChildNodes.OfType<XmlNode>())
            {
                processNode(_item, pad + "  ");
            }
        }

        private void ProcessCData(XmlNode x)
        {
            var _parent = x.ParentNode;
            var _section = x as XmlCDataSection;
            if (_parent.Name == "sql-query")
            {
                var _name = _parent.Attributes["name"].Value;
                var _callable = GetCallable(_parent);
                var pureName = Path.GetFileNameWithoutExtension(fileName);
                var possibleExt = Path.GetExtension(pureName);
                if (".hbm".Equals(possibleExt, StringComparison.InvariantCultureIgnoreCase))
                {
                    pureName = Path.GetFileNameWithoutExtension(pureName);
                }
                var _fnamePrefix = string.Format("{0}.{1}", pureName, _name);
                var suffix = "";
                if (!_callable)
                {
                    suffix = ".obs";
                }
                var _sData = _section.Data.Trim();
                if (!string.IsNullOrEmpty(_sData))
                {
                    var _fname = string.Format("{0}{1}{2}", _fnamePrefix, suffix, Ext);
                    var _ctr = 1;
                    var _fullName = Path.Combine(outPath, _fname);
                    while (File.Exists(_fullName))
                    {
                        _fname = string.Format("{0}${1}{2}{3}", _fnamePrefix, _ctr++, suffix, Ext);
                        _fullName = Path.Combine(outPath, _fname);
                    }
                    File.WriteAllText(_fullName, _sData);
                }
            }
        }

        private bool GetCallable(XmlNode parent)
        {
            var attr = parent.Attributes["callable"];
            if (attr == null)
            {
                return true;
            }
            var data = attr.Value;
            try
            {
                var res = Boolean.Parse(data);
                if (!res)
                {
                }
                return res;
            }
            catch ( Exception e )
            {
            }
            return true;
        }

        private void ProcessSqlQuery(XmlNode x)
        {
        }

    }
}
