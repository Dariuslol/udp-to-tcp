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
            // own ip
            var receivingEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 5010);
            var receivingEP = (EndPoint)receivingEndpoint;
    

            // server ip
            var remoteEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 5004);


            HelloMSG h = new HelloMSG();
            h.Type = Messages.HELLO;
            h.To = serverIP;
            h.From = localIP;

            RequestMSG r = new RequestMSG();
            r.Type = Messages.REQUEST;
            r.To = serverIP;
            r.From = localIP;
            r.FileName = "test.txt";

            DataMSG D = new DataMSG();
            D.Type = Messages.DATA;
            D.To = serverIP;
            D.From = localIP;

            AckMSG ack = new AckMSG();
            ack.Type = Messages.ACK;
            ack.To = serverIP;
            ack.From = localIP;

            CloseMSG cls = new CloseMSG();
            cls.Type = Messages.CLOSE_REQUEST;
            cls.To = serverIP;
            cls.From = localIP;

            try
            {
                // TODO: Instantiate and initialize your socket 
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                

                // TODO: Send hello mesg
                Console.Write("INFO:\tsending hello message to server... ");
                msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(h));
                sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEndpoint);
                Console.WriteLine("done");


                // TODO: Receive and verify a HelloMSG 
                Console.Write("INFO:\tWaiting for server HELLO_REPLY message... ");
                var helloReplyBytes = sock.ReceiveFrom(buffer, ref receivingEP);
                var helloReplyString = Encoding.ASCII.GetString(buffer, 0, helloReplyBytes);
                var helloReply = JsonSerializer.Deserialize<HelloMSG>(helloReplyString);

                if (helloReply.Type != Messages.HELLO_REPLY) 
                {
                    Console.WriteLine("ERROR:\tGot response that is not a HELLO_REPLY");
                    return;
                }
                Console.WriteLine($"received with ConID: {helloReply.ConID}");
                

                // TODO: Send the RequestMSG message requesting to download a file name
                Console.Write("INFO:\tsending request message to server... ");
                r.ConID = helloReply.ConID;
                msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(r));
                sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEndpoint);
                Console.WriteLine("done");

                
                // TODO: Receive a RequestMSG from remoteEndpoint
                // receive the message and verify if there are no errors
                Console.Write("INFO:\tWaiting for server REQUEST_REPLY message... ");
                var requestReplyBytes = sock.ReceiveFrom(buffer, ref receivingEP);
                var requestReplyJson = Encoding.ASCII.GetString(buffer, 0, requestReplyBytes);
                var requestReply = JsonSerializer.Deserialize<HelloMSG>(requestReplyJson);

                if (requestReply.Type != Messages.REPLY) 
                {
                    Console.WriteLine("ERROR Got response that is not a REQUEST_REPLY" + requestReply.Type);
                    return;
                }
                Console.WriteLine($"received");

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
