using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras
{
    public class Config
    {
        private string HOSTNAME { get; set; }
        private int DB_PORT { get; set; }
        private string DB_ORG { get; set; } 
        private string DB_BUCKET { get; set; }
        private string TOKEN { get; set; }
        private string DB { get; set; }
        private string configFile { get; set; } = "./config.txt"; 


        public Config() {

            List<string> list = readConfigFile();
            HOSTNAME = list[0];
            DB_PORT= Int32.Parse(list[1]);
            DB_ORG = list[2];
            DB_BUCKET = list[3];
            TOKEN = list[4];
            DB = "http://" + HOSTNAME + ":" + DB_PORT;

            CreateBucketIfNotExistsAsync(DB_BUCKET, DB, TOKEN, DB_ORG, 86400);

        }

        private static async Task<bool> CreateBucketIfNotExistsAsync(string bucketName, string url, string token, string org, int retentionSeconds)
        {
            try
            {
                using var client = InfluxDBClientFactory.Create(url, token);
                string orgId = (await client.GetOrganizationsApi().FindOrganizationsAsync(org: org)).First().Id;
                var bucket = await client.GetBucketsApi().FindBucketByNameAsync(bucketName);

                if (bucket != null)
                {
                    return true;
                }
                else
                {
                    var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, retentionSeconds);
                    await client.GetBucketsApi().CreateBucketAsync(bucketName, retention, orgId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating bucket: {ex.Message}");
                //Logger.Log($"An error occurred while creating bucket: {ex.Message}");
                return false;
            }
        }

        public string getHOSTNAME() { return HOSTNAME; } 
        public int getDB_PORT() { return DB_PORT; }
        public string getDB_ORG() { return DB_ORG;  }
        public string getDB_BUCKET() { return DB_BUCKET; }
        public string getDB() { return DB; }
        public string getTOKEN() {  return TOKEN; }

        private List<string> readConfigFile()
        {
            string line = "";
            List<string> lines = new List<string>();
            using (var streamReader = new StreamReader(configFile))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] str = line.Split(':');
                    string s = str[1].Trim();
                    lines.Add(s);
                }

            }
            
            return lines;
        }


        // Destructor
        ~Config()
        {

        }
    }
}
