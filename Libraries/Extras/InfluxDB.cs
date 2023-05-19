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
        public override string ToString()
        {
            return string.Format("Token: {0}, bucket: {1}, org: {2}, DB: {3}", Token, bucket, org, DB);
        }

        public async Task SendToInfluxDB(string MsgName, Dictionary<string, double> Data)
        {
            try
            {
                DBClass dc = new DBClass();
                //sConsole.WriteLine(dc.ToString());

                using var client = new InfluxDBClient(dc.DB, dc.Token);
                var writeApi = client.GetWriteApiAsync();
                Console.WriteLine("Writing " + MsgName + " to database......");
                //Logger.Log("Writing " + MsgName + " to database......");
                foreach (var item in Data)
                {
                    var point = PointData.Measurement(MsgName)
                                    .Tag("Signal", item.Key)
                                    .Field("Value", item.Value)
                                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
                    await writeApi.WritePointAsync(point, dc.bucket, dc.org);
                }
                Console.WriteLine(MsgName + ": Completed!");
                //Logger.Log(MsgName + ": Completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                //Logger.Log("Error occurred: " + ex.Message);
            }


        }

    }
}
