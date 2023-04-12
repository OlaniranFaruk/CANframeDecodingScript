using CanFrameLib;
using Extras;
using DbcParserLib;
using DbcParserLib.Model;
using System.Text.RegularExpressions;

namespace script
{
    class Program
    {
        static void Main()
        {
            //string dbcFilePath = "C:\\Damola\\Thesis\\Influx\\MODEL3CAN.dbc";
            string canDataSrc = "/var/run/rexgen/can0/rx";
            //string dbcFilePath = "C:\\Damola\\Thesis\\Influx\\E_Coach_Working_CAN_DB_File_Export_PACN_20221209.dbc";
            //string dbcFilePath = "../../../../E_Coach_Working_CAN_DB_File_Export_PACN_20221209.dbc";
            //string dbcFilePath = "C:\\Damola\\Thesis\\Influx\\SampleCANdata.dbc";
            string dbcFilePath, line;
            bool messageFound = false;
            Dbc Dbc;
            string[] words;

            //Get DBC file
            dbcFilePath = MyFunctions.requestDbcFileName();
            

            //List of selected signals from application
            List<string> SelectedSignalsList = new List<string>();

            //read content of dbc file
            Console.WriteLine("Reading from " + dbcFilePath + " .......");
            Dbc = Parser.ParseFromPath(dbcFilePath);
            Console.WriteLine("DBC file read successfully.");
            Console.WriteLine("Receiving data on data pipe: "+canDataSrc);
            while (true)
            {
                using (var streamReader = new StreamReader(canDataSrc))
                {
                    
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line != null)
                        {
                            Console.WriteLine("CAN Frame: " + line);
                            words = Regex.Split(line, @"[\(\)\[\]\s]+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                            CANFrame canframe = new CANFrame(words);

                            foreach (Message msg in Dbc.Messages)
                            {
                                if (msg.ID == (canframe.Identifier & 0x1FFFFFFF))
                                {
                                    Console.WriteLine(System.String.Format("ID match found: {0} ({1:x3})", canframe.Identifier, canframe.Identifier));
                                    Console.WriteLine(msg.Name + " Signals:");
                                    messageFound = true;
                                    Dictionary<string, double>  decodeValues = MyFunctions.DecodeFrame(canframe, msg);
                                    Console.WriteLine("Writing to database......");
                                    InfluxDBClass.SendToInfluxDB(msg.Name, decodeValues);
                                    Console.WriteLine("Completed!");
                                    break;
                                }


                            }
                            if (messageFound == false)
                            {
                                Console.WriteLine(System.String.Format("No match found for ID : {0} ({1:x3})", canframe.Identifier, canframe.Identifier));
                            }
                            else
                            {
                                messageFound = false;
                            }

                            //Console.WriteLine(canframe);
                            //Console.WriteLine("To string function: " + canframe.ToString());
                            //Thread.Sleep(1500);
                        }

                    }

                }
            }

        }

    }
}

