using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nest;

namespace PostgresLogAnalyzer
{
    class Program
    {
        private static Uri node = new Uri("http://localhost:9200");
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid command line. Please use:");
                Console.WriteLine(" {0} path_to_log_files", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("Press return to continue...");
                Console.ReadLine();
                return 1;
            }
            var path = args[0];
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Directory {0} is not found", path);
                Console.WriteLine("Press return to continue...");
                Console.ReadLine();
                return -1;
            }
            var settings = new ConnectionSettings(
                node,
                defaultIndex: "pg-logs"
                );
            var client = new ElasticClient(settings);
            client.DeleteIndex("*");
            client.Refresh();
            var fp = Path.GetFullPath(path).TrimEnd('\\');
            foreach (var file in Directory.EnumerateFiles(fp, "*.log", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file);
                if (fileName == null)
                    continue;
                // adjust index name
                List<string> prefixes = new List<string>();
                var root = Path.GetDirectoryName(file);
                while (!string.IsNullOrEmpty(root))
                {
                    if (root == fp)
                    {
                        break;
                    }
                    var prefix = Path.GetFileName(root);
                    prefixes.Add(prefix);
                    root = Path.GetDirectoryName(root);
                }
                var fnamePrefix = string.Join("-", prefixes);
                var indexName = fileName;
                if (!string.IsNullOrEmpty(fnamePrefix))
                {
                    indexName = string.Format("{0}-{1}", fnamePrefix, indexName);
                }
                processFile(file, client, indexName);
            }
            Console.WriteLine("Press return to continue...");
            Console.ReadLine();
            return 0;
        }

        private static void processFile(string fname, ElasticClient client, string indexName)
        {
            Console.WriteLine("Source file: {0}", fname);
            var parser = new LogParser();
            var lines = parser.Load(fname);
            Console.WriteLine("Parsed {0} lines", lines);
            // save it
            SaveIt(indexName, parser, client);
        }


        private static void SaveIt(string fname, LogParser parser, ElasticClient client)
        {
            var index = fname;
            Console.WriteLine("Indexing {0}", index);
            foreach (var item in parser.Items)
            {
                client.Index(item, i => i.Index(index));
            }
            Console.WriteLine("Refresh {0}", index);
            client.Refresh(); // required!!!
            Console.WriteLine("Analyze {0}", index);
            var stat = client.IndicesStats();
            if (!stat.Indices.ContainsKey(index))
            {
                Console.WriteLine("Index {0} is not found", index);
            }
            else
            {
                var idx = stat.Indices[index];
                Console.WriteLine("Documents:{0}", idx.Total.Documents.Count);
                ProcessResponse("query", client.Search<LogItem>(body =>
                    body
                        .Index(index)
                        .Query(
                            query =>
                                query.QueryString(
                                    qs => qs.
                                        OnFields(f => f.Stamp)
                                        .Query("47")))));
                ProcessAggResponse("group_by_kinds", client.Search<LogItem>(body =>
                    body
                        .Index(index)
                        .Aggregations(aa => 
                            aa.Terms("group_by_kinds", ts => 
                                ts.Field( o => o.Kind )
                                .Aggregations( aa2 => 
                                   aa2.Cardinality("kinds", cc => cc.Field(p => p.Stamp)))))));
                ProcessAggResponse("id1", client.Search<LogItem>(body =>
                    body
                        .Index(index)
                        .Aggregations(aa => aa.Cardinality("id1", cc => cc.Field(p => p.Id1)))));
                ProcessAggResponse("ip", client.Search<LogItem>(body =>
                    body
                        .Index(index)
                        .Aggregations(aa => aa.Cardinality("ip", cc => cc.Field(p => p.Ip)))));
            }
        }

        private static void ProcessResponse(string data, ISearchResponse<LogItem> searchResults)
        {
            Console.WriteLine("===== Search results {0} =====", data);
            if (!searchResults.IsValid)
            {
                Console.WriteLine("Invalid response!!!");
                return;
            }
            if (searchResults.Documents.Any())
            {
                Console.WriteLine("We have docs: {0}", searchResults.Documents.Count());
                foreach (var item in searchResults.Documents)
                {
                    Console.WriteLine(" {0}", item);
                }
            }
            if (searchResults.Facets.Any())
            {
                Console.WriteLine("We have facets: {0}", searchResults.Facets.Count());
                foreach (var item in searchResults.Facets)
                {
                    Console.WriteLine(" {0}", item);
                }
            }
            if (searchResults.Hits.Any())
            {
                Console.WriteLine("We have hits: {0}", searchResults.Hits.Count());
                foreach (var item in searchResults.Hits)
                {
                    Console.WriteLine(" Id:{0} Index:{1}", item.Id, item.Index);
                }
            }
            if (searchResults.Shards.Total != 0)
            {
                Console.WriteLine("Total Shards: {0}", searchResults.Shards.Total);
                Console.WriteLine("Total Successful: {0}", searchResults.Shards.Successful);
                Console.WriteLine("Total Failed: {0}", searchResults.Shards.Failed);
                Console.WriteLine("Total Failures: {0}", searchResults.Shards.Failures);
            }
        }
        private static void ProcessAggResponse(string data, ISearchResponse<LogItem> searchResults)
        {
            Console.WriteLine("===== Agg results {0} =====", data);
            if (!searchResults.IsValid)
            {
                Console.WriteLine("Invalid response!!!");
                return;
            }
            foreach (var item in searchResults.Aggregations)
            {
                Console.WriteLine("Aggregation: {0}", item.Key);
                var val = item.Value;
                if (val is Nest.ValueMetric)
                {
                    ProcessMetric(val as Nest.ValueMetric);
                }
                else if (val is Nest.Bucket)
                {
                    ProcessBucket(val as Nest.Bucket);
                }
                else
                {
                    throw new Exception(string.Format("Unexpected val type {0}", val));
                }
            }
        }

        private static void ProcessBucket(Bucket bucket)
        {
            foreach (var item in bucket.Items)
            {
                if (item is Nest.KeyItem)
                {
                    ProcessKeyItem(item as KeyItem);
                }
                else
                {
                    throw new Exception(string.Format("Unexpected val type {0}", item));
                }
            }
        }

        private static void ProcessKeyItem(KeyItem keyItem)
        {
            Console.WriteLine("Key:{0} DocCount: {1}", keyItem.Key, keyItem.DocCount);
        }

        private static void ProcessMetric(ValueMetric valueMetric)
        {
            Console.WriteLine("Value:{0}", valueMetric.Value);
        }

    }
}
