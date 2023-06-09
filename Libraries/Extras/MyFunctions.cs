﻿using CanFrameLib;
using DbcParserLib;
using DbcParserLib.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Extras
{
    public class MyFunctions
    {

        static public Dictionary<string, double> DecodeFrame(CANFrame canframe, Message msg, List<string> SelectedSignalsList)
        {
            
            Dictionary<string, double> values = new Dictionary<string, double>();
            string hexstring = LittleEndian(canframe.Payload);
            //List of selected signals from application

            // Process line

            int loopItr = 0;
            //decode only message with data length 8
            if (msg.DLC == 8)
            {
                // Create an empty dictionary to hold the decoded signal values
                

                foreach (Signal signal in msg.Signals)
                {
                    if (SelectedSignalsList.Contains(signal.Name))
                    {
                        //convert the byte array to hexstring                    
                        


                        double signalValue = 0;
                        //if the length is greater than 8 , it is spread over more than one byte
                        //and if the byte order is 1 , perform a byte swap
                        if (signal.Length > 8 && signal.ByteOrder == 1)
                        {
                            string s = "";
                            //loopItr defines how many times to go through the hex string
                            //*2 because one byte is represented by 2 characters
                            loopItr = (signal.Length / 8) * 2;
                            if (signal.Length % 8 != 0)
                            {
                                loopItr = ((signal.Length / 8) + 1) * 2;
                            }

                            /*
                             * 
                              Divide startbit by 8 to get the start Byte
                              Take the following characters after the start byte including the start byte for the length of the loopitr
                             */
                            for (int i = 0; i < loopItr; i++)
                            {
                                s += hexstring[(signal.StartBit / 8) + i];
                            }
                            //swap the bytes
                            string newstr = "";
                            /* 
                             Example
                            Input : FD 12 34
                            Output: 34 12 FD
                             */
                            for (int i = s.Length - 1; i >= 1; i = i - 2)
                            {
                                newstr += s[i - 1];
                                newstr += s[i];
                            }
                            //convert the new swapped extract to int
                            long longValue = Convert.ToInt64(newstr, 16);
                            //copy the data from the swapped bits
                            byte b = BitwiseCopy(longValue, 0, 0, signal.Length);
                            signalValue = b;
                        }
                        else
                        {
                            //convert the hexstring to integer for easy bit shifting
                            long hexstringInInt = Convert.ToInt64(hexstring, 16);

                            byte b = BitwiseCopy(hexstringInInt, signal.StartBit, 0, signal.Length);
                            signalValue = b;
                        }

                        signalValue = (signalValue * signal.Factor) + signal.Offset;
                        if (signalValue >= signal.Minimum && signalValue <= signal.Maximum)
                        {
                            Console.WriteLine(System.String.Format("{0} = {1} {2}", signal.Name, signalValue, signal.Unit));
                            //Logger.Log(System.String.Format("{0} = {1} {2}", signal.Name, signalValue, signal.Unit));
                        }
                        else
                        {
                            Console.WriteLine(System.String.Format("{0} = {1} {2} (Not in range )", signal.Name, signalValue, signal.Unit));
                            //Logger.Log(System.String.Format("{0} = {1} {2} (Not in range )", signal.Name, signalValue, signal.Unit));
                        }
                        values.Add(signal.Name, signalValue);
                    }
                    else
                    {
                        Console.WriteLine(signal.Name + " (Skipped) " );
                        //Logger.Log(signal.Name + " (Skipped) ");
                    }
                    
                }

               
            }
            else
            {
                Console.WriteLine(msg.Name+ "'s DLC is greater than 8.");
                //Logger.Log(msg.Name + "'s DLC is greater than 8.");
            }
            return values;
        }
        static public Dictionary<string, double> returnNewSignals(Dictionary<string, double> decodeValues, List<string> FormulaList, string msgName)
        {
            Dictionary<string, double> calSignalsList = new Dictionary<string, double>();
            string[] strs;
            string mName, rest;
            foreach (string item in FormulaList)
            {
                /* 
                  formula syntax:
                    [message name]:[new signal name] = [signal][+*-/%][signal]....
                 */
                
                strs = item.Split(':');
                mName = strs[0];    
                rest = strs[1];
                if (msgName.Equals(mName))
                {
                    //get the new signal name
                    string[] strings = rest.Split('=');
                    string newSignalName = strings[0];
                    double newSignalValue = CalculateSignal(strings[1], decodeValues);
                    calSignalsList.Add(newSignalName, newSignalValue);
                }
            }

            return calSignalsList;
        }

        static double CalculateSignal(string forumlaStr, Dictionary<string, double> decodeValues)
        {
            double returnValue = 0, val;
            string str, symbol = " ";
            string[] formula = forumlaStr.Split(' ');

            str = formula[0].Trim();
            returnValue = Convert.ToDouble(decodeValues[str]);
            for (int i = 1; i < formula.Length; i++)
            {
                str = formula[i].Trim();
                if (!IsOperator(str))
                {  
                    switch (symbol)
                    {
                        case "+":
                            str = formula[i + 1].Trim();
                            if (!Double.TryParse(str, out val))
                            {
                                val = Convert.ToDouble(decodeValues[str]);
                            }
                            returnValue += val;
                            break;
                        case "*":
                            str = formula[i + 1].Trim();
                            if (!Double.TryParse(str, out val))
                            {
                                val = Convert.ToDouble(decodeValues[str]);
                            }
                            returnValue *= val;
                            break;
                        case "-":
                            str = formula[i + 1].Trim();
                            if (!Double.TryParse(str, out val))
                            {
                                val = Convert.ToDouble(decodeValues[str]);
                            }
                            returnValue -= val;
                            break;
                        case "/":
                            str = formula[i + 1].Trim();
                            if (!Double.TryParse(str, out val))
                            {
                                val = Convert.ToDouble(decodeValues[str]);
                            }
                            returnValue /= val;
                            break;
                        case "%":
                            str = formula[i + 1].Trim();
                            if (!Double.TryParse(str, out val))
                            {
                                val = Convert.ToDouble(decodeValues[str]);
                            }
                            returnValue %= val;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    symbol = str;   
                }
            }
            return returnValue;
        }

        static bool IsOperator(string input)
        {
            Regex regex = new Regex(@"^[-+*/%]$");
            return regex.IsMatch(input);
        }
        static string LittleEndian(string num)
        {
            long number = Convert.ToInt64(num, 16);
            byte[] bytes = BitConverter.GetBytes(number);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2");
            return retval;
        }

        static public byte BitwiseCopy(long value, int sourceStartBit, int destStartBit, int bitCount)
        {
            byte result = (byte)((value >> sourceStartBit) << destStartBit);
            result &= (byte)~(0xff << bitCount); // mask for zeros at the left of result
            return result;
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
