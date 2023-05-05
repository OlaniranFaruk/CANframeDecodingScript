using InfluxDB.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

namespace Extras
{
    public class DBClass
    {
        
        private string Token;
        private string bucket;
        private string org;
        private string DB;

        public DBClass() {
            Config conf = new Config();

            Token = conf.getTOKEN();
            bucket = conf.getDB_BUCKET();
            org = conf.getDB_ORG();
            DB = conf.getDB();

        }

        public static async Task SendToInfluxDB(string MsgName, Dictionary<string, double> Data)
        {
            DBClass dc = new DBClass();

            using var client = new InfluxDBClient(dc.DB, dc.Token);
            var writeApi = client.GetWriteApiAsync();
            Console.WriteLine("Writing to database......");
            foreach (var item in Data)
            {
                var point = PointData.Measurement(MsgName)
                                .Tag("Signal", item.Key)
                                .Field("Value", item.Value)
                                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
               await writeApi.WritePointAsync(point, dc.bucket, dc.org);
            }
            Console.WriteLine("Completed!");


        }

    }
}
