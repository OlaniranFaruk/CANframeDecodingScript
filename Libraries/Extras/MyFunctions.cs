using CanFrameLib;
using DbcParserLib;
using DbcParserLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Extras
{
    public class MyFunctions
    {

        static public Dictionary<string, double> DecodeFrame(CANFrame canframe, Message msg)
        {


            // Create an empty dictionary to hold the decoded signal values
            Dictionary<string, double> values = new Dictionary<string, double>();


            // Loop through each signal defined for this message ID
            foreach (Signal signal in msg.Signals)
            {


                // Extract the signal data bytes from the message data
                byte[] signalData = new byte[signal.Length / 8 + ((signal.Length % 8) > 0 ? 1 : 0)];
                int startIndex = msg.IsExtID ? (signal.StartBit + 16) : signal.StartBit;
                Array.Copy(canframe.Payload, startIndex / 8, signalData, 0, signalData.Length);

                // Extract the signal value from the signal data bytes
                ulong signalValue = 0;
                for (int i = 0; i < signalData.Length; i++)
                {
                    signalValue <<= 8;
                    signalValue |= signalData[i];
                }
                signalValue >>= (8 - (startIndex % 8) - signal.Length);

                // Apply the scaling factor and offset to the signal value
                double scaledValue = (signalValue * signal.Factor) + signal.Offset;

                // Add the signal value to the dictionary of decoded values
                values.Add(signal.Name, scaledValue);
                Console.WriteLine(System.String.Format("{0} = {1} {2}", signal.Name, signalValue, signal.Unit));
            }

            // Return the dictionary of decoded signal values
            return values;


        }

        static public string requestDbcFileName()
        {
            string dbcFilePath;
            //request dbc file from user
            Console.WriteLine("\nEnter a path to dbc file: ");
            dbcFilePath = Console.ReadLine();
            while (true)
            {
                if (string.IsNullOrWhiteSpace(dbcFilePath))
                {
                    Console.WriteLine("\nKindly enter a valid dbc file (/path/to/dbc) :");
                    dbcFilePath = Console.ReadLine();
                }
                else
                {
                    //check if the file name provided is a dbc file
                    string[] s = dbcFilePath.Split('.');
                    if (s[s.Length - 1] != "dbc")
                    {
                        Console.WriteLine("\nFile needs to be a .dbc :");
                        dbcFilePath = Console.ReadLine();
                    }
                    else
                    {
                        if (File.Exists(dbcFilePath))
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("\nFile does not exists :");
                            dbcFilePath = Console.ReadLine();
                        }
                    }
                }

            }

            return dbcFilePath;
        }
    }
}
