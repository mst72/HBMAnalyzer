using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nest;
using PostgresLogAnalyzer;

namespace ElasticView.Model
{
    public class MainModel
    {
        private static Uri node = new Uri("http://localhost:9200");
        private ElasticClient client = null;
        internal bool Connect()
        {
            var settings = new ConnectionSettings(
                node,
                defaultIndex: "pg-logs"
                );
            client = new ElasticClient(settings);
            return true;
        }

        internal void ExecuteSomething()
        {
            ProcessResponse("query", client.Search<LogItem>(body =>
                body
                    .AllIndices()
                    .Query(
                        query =>
                            query.QueryString(
                                qs => qs.
                                    OnFields(f => f.Stamp)
                                    .Query("47")))));
            ProcessAggResponse("group_by_kinds", client.Search<LogItem>(body =>
                body
                    .AllIndices()
                    .Aggregations(aa =>
                        aa.Terms("group_by_kinds", ts =>
                            ts.Field(o => o.Kind)
                            .Aggregations(aa2 =>
                               aa2.Cardinality("kinds", cc => cc.Field(p => p.Stamp)))))));
            ProcessAggResponse("group_by_sub_kinds", client.Search<LogItem>(body =>
                body
                    .AllIndices()
                    .Aggregations(aa =>
                        aa.Terms("group_by_sub_kinds", ts =>
                            ts.Field(o => o.SubKind)
                            .Aggregations(aa2 =>
                               aa2.Cardinality("subKinds", cc => cc.Field(p => p.Stamp)))))));
            ProcessAggResponse("id1", client.Search<LogItem>(body =>
                body
                    .AllIndices()
                    .Aggregations(aa => aa.Cardinality("id1", cc => cc.Field(p => p.Id1)))));
            ProcessAggResponse("ip", client.Search<LogItem>(body =>
                body
                    .AllIndices()
                    .Aggregations(aa => aa.Cardinality("ip", cc => cc.Field(p => p.Ip)))));
        }

        private static void ProcessResponse(string data, ISearchResponse<LogItem> searchResults)
        {
            Console.WriteLine("===== Search results {0} =====", data);
            Console.WriteLine("Request:   {0}", System.Text.Encoding.UTF8.GetString( searchResults.ConnectionStatus.Request ));
            // Console.WriteLine("Response:  {0}", searchResults.ConnectionStatus.ResponseRaw.ToString());
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
            Console.WriteLine("Request:   {0}", System.Text.Encoding.UTF8.GetString(searchResults.ConnectionStatus.Request));
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
