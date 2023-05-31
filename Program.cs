using CanFrameLib;
using Extras;
using DbcParserLib;
using DbcParserLib.Model;
using System.Text.RegularExpressions;
using System.IO;

namespace script
{
    class Program
    {
        static void Main()
        {
            string canDataSrc = "/var/run/rexgen/can0/rx";
            string filename = "./SelectedSignals.txt";
            string dbcFilePath = "./Dbc-file.dbc";
            string CalcFilePath = "./CalcSignals.txt";
            string line;
            bool messageFound = false;
            Dbc Dbc;
            string[] words;
            List<string> SelectedSignalsList = new List<string>();
            List<string> CalcSignalsList = new List<string>();


            //Get DBC file
            //dbcFilePath = MyFunctions.requestDbcFileName();
            
                        
            //read content of dbc file
            Console.WriteLine("Reading from " + dbcFilePath + " .......");
            //Logger.Log("Reading from " + dbcFilePath + " .......");
            Dbc = Parser.ParseFromPath(dbcFilePath);
            Console.WriteLine("DBC file read successfully.");
            //Logger.Log("DBC file read successfully.");
            DBClass database = new DBClass();
            
            //get the selected signals and add to list
            using (var sr = new StreamReader(filename))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    SelectedSignalsList.Add(line);
                }
            }

            //get the formulas of the calculated signals
            using (var sr = new StreamReader(CalcFilePath))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    CalcSignalsList.Add(line);
                }
            }

            Console.WriteLine("Receiving data on data pipe: "+canDataSrc);
            //Logger.Log("Receiving data on data pipe: " + canDataSrc);
            while (true)
            {
                using (var streamReader = new StreamReader(canDataSrc))
                {
                    
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line != null)
                        {
                            
                            words = Regex.Split(line, @"[\(\)\[\]\s]+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                            CANFrame canframe = new CANFrame(words);
                            Console.WriteLine("CAN Frame: " + canframe.ToString());
                            //Logger.Log("CAN Frame: " + canframe.ToString());

                            foreach (Message msg in Dbc.Messages)
                            {
                                if (msg.ID == (canframe.Identifier & 0x1FFFFFFF))
                                {
                                    Console.WriteLine(System.String.Format("ID match found: {0} ({1:x3})", canframe.Identifier, canframe.Identifier));
                                    //Logger.Log(System.String.Format("ID match found: {0} ({1:x3})", canframe.Identifier, canframe.Identifier));


                                    Console.WriteLine(msg.Name + " Signals:");
                                    //Logger.Log(msg.Name + " Signals:");
                                    messageFound = true;
                                    Dictionary<string, double>  decodeValues = MyFunctions.DecodeFrame(canframe, msg, SelectedSignalsList);
                                    
                                    if (decodeValues.Count != 0)
                                    {
                                        Dictionary<string, double> d = MyFunctions.returnNewSignals(decodeValues, CalcSignalsList, msg.Name);

                                        foreach (KeyValuePair< string, double> item in d)
                                        {
                                            decodeValues.Append(item);
                                        }

                                        database.SendToInfluxDB(msg.Name, decodeValues);
                                    }
                                    
                                    break;
                                }


                            }
                            if (messageFound == false)
                            {
                                Console.WriteLine(System.String.Format("No match found for ID : {0} ({1:x3})", canframe.Identifier, canframe.Identifier));
                                //Logger.Log(System.String.Format("No match found for ID : {0} ({1:x3})", canframe.Identifier, canframe.Identifier));
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

