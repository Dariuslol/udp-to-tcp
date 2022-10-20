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
            string serverIP = "127.0.0.1";
            string localIP = "127.0.0.1";

            // TODO: add the student number of your group members as a string value. 
            // string format example: "Jan Jansen 09123456" 
            // If a group has only one member fill in an empty string for the second student
            string student_1 = "Darius van Essen 1026751";
            string student_2 = "Nino Reichardt 1038779";

            byte[] buffer = new byte[1000];
            byte[] msg = new byte[100];
            Socket sock;

            // TODO: Initialise the socket/s as needed from the description of the assignment
            var serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 5004);

            var localEndpoint = new IPEndPoint(IPAddress.Any, 5010);
            var localEP = (EndPoint)localEndpoint;

            HelloMSG h = new HelloMSG();
            h.To = serverIP;
            h.From = localIP;
                
            RequestMSG r = new RequestMSG();
            h.To = serverIP;
            h.From = localIP;

            DataMSG D = new DataMSG();
            AckMSG ack = new AckMSG();
            CloseMSG cls = new CloseMSG();

            try
            {
                // TODO: Instantiate and initialize your socket 
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                

                // TODO: Send hello mesg
                h.From = "127.0.0.1";
                h.To = "127.0.0.1";
                h.Type = Messages.HELLO;

                msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(h));
                sock.SendTo(msg, msg.Length, SocketFlags.None, serverEndpoint);

                // TODO: Receive and verify a HelloMSG 
                var helloReply = JsonSerializer.Deserialize<HelloMSG>(Encoding.ASCII.GetString(buffer, 0, sock.ReceiveFrom(buffer, ref localEP)));
                if (helloReply.Type != Messages.HELLO_REPLY) 
                {
                    Console.WriteLine("Got response that is NOT HELLO_REPLY");
                    return;
                }

                // TODO: Send the RequestMSG message requesting to download a file name
                r.Type = Messages.REQUEST;

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
