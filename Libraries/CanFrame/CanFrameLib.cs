using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Text;

namespace CanFrameLib
{
    public class CANFrame
    {
        public float Timestamp { get; set; }     // Timestamp of the frame in milliseconds
        public string CANInterface { get; set; }   // CAN bus interface or channel
        public uint Identifier { get; set; }    // CAN identifier or message ID

        public int DataLength { get; set; }     // Length of CAN data or payload
        public byte[] Payload { get; set; }  // CAN data payload or message data

        
        //CANFrame constructor
        public CANFrame(string[] canframe)
        {
            Timestamp = float.Parse(canframe[0]);
            CANInterface = canframe[1];
            Identifier = uint.Parse(canframe[2], System.Globalization.NumberStyles.HexNumber);
            DataLength = int.Parse(canframe[3]);

            string payload = "";
            for (int i = 0; i < DataLength; i++)
            {
                payload += canframe[4 + i];
            }
            Payload = Encoding.ASCII.GetBytes(payload);
        }

        public CANFrame() { }

        public override string ToString()
        {
            return System.String.Format("({0}) {1} {2:x3} [{3}] {4}", Timestamp, CANInterface, Identifier, DataLength, Encoding.ASCII.GetString(Payload));
        }




    }




}