using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UDP_FTP.Models;
using UDP_FTP.Error_Handling;
using static UDP_FTP.Models.Enums;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: add the student number of your group members as a string value. 
            // string format example: "Jan Jansen 09123456" 
            // If a group has only one member fill in an empty string for the second student
            string student_1 = ;
            string student_2 = ;


            byte[] buffer = new byte[1000];
            byte[] msg = new byte[100];
            Socket sock;
            // TODO: Initialise the socket/s as needed from the description of the assignment

            HelloMSG h = new HelloMSG();
            RequestMSG r = new RequestMSG();
            DataMSG D = new DataMSG();
            AckMSG ack = new AckMSG();
            CloseMSG cls = new CloseMSG();

            try
            {
                // TODO: Instantiate and initialize your socket 


                // TODO: Send hello mesg

                // TODO: Receive and verify a HelloMSG 


                // TODO: Send the RequestMSG message requesting to download a file name

                // TODO: Receive a RequestMSG from remoteEndpoint
                // receive the message and verify if there are no errors


                // TODO: Check if there are more DataMSG messages to be received 
                // receive the message and verify if there are no errors

                // TODO: Send back AckMSG for each received DataMSG 


                // TODO: Receive close message
                // receive the message and verify if there are no errors

                // TODO: confirm close message

            }
            catch
            {
                Console.WriteLine("\n Socket Error. Terminating");
            }

            Console.WriteLine("Download Complete!");
           
        }
    }
}
