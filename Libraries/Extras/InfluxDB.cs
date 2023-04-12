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
    public class InfluxDBClass
    {
        private const string Token = "mjdBY4uUVXtyGIbWG7TSFfa5reLpGDq_8cFs7bsaTeS78BquCuUD8cNXV7dI0VSz2en19sEwmobYKy3AHsky0A==";
        private const string bucket = "TEST";
        private const string org = "InfluxTech";
        private const string usr = "root";
        private const string pwd = "password";
        private const string DB = "http://192.168.134:8086";

        public static async Task SendToInfluxDB(string MsgName, Dictionary<string, double> Data)
        {
            using var client = new InfluxDBClient(DB, Token);
            var writeApi = client.GetWriteApiAsync();

            foreach (var item in Data)
            {
                var point = PointData.Measurement(MsgName)
                                .Tag("Signal", item.Key)
                                .Field("Value", item.Value)
                                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
               await writeApi.WritePointAsync(point, bucket, org);
            }


        }

    }
}
