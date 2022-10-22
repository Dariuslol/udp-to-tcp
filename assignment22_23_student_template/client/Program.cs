using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Linq;
using Client.Models;
using Client.Error_Handling;
using System.Globalization;
using static Client.Models.Enums;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIP = "127.0.0.1";
            string localIP = "127.0.0.1";

            int[] failOn = { 3, 4, 9, 13, 15, 34 };

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

            AckMSG ack = new AckMSG();
            ack.Type = Messages.ACK;
            ack.To = serverIP;
            ack.From = localIP;

            CloseMSG cls = new CloseMSG();
            cls.Type = Messages.CLOSE_CONFIRM;
            cls.To = serverIP;
            cls.From = localIP;
            


            ConSettings conSettings = new();
            conSettings.From = serverIP;
            conSettings.To = localIP;

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

                ErrorHandler.VerifyGreeting(helloReply, conSettings);
                Console.WriteLine("received!");
                conSettings.ConID = helloReply.ConID;

                r.ConID = conSettings.ConID;
                ack.ConID = conSettings.ConID;
                cls.ConID = conSettings.ConID;

                // TODO: Send the RequestMSG message requesting to download a file name
                Console.Write("INFO:\tsending request message to server... ");
                msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(r));
                sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEndpoint);
                Console.WriteLine("done!");

                
                // TODO: Receive a RequestMSG from remoteEndpoint
                // receive the message and verify if there are no errors
                Console.Write("INFO:\tWaiting for server REQUEST_REPLY message... ");
                var requestReplyBytes = sock.ReceiveFrom(buffer, ref receivingEP);
                var requestReplyJson = Encoding.ASCII.GetString(buffer, 0, requestReplyBytes);
                var requestReply = JsonSerializer.Deserialize<RequestMSG>(requestReplyJson);

                ErrorHandler.VerifyRequest(requestReply, conSettings);
                Console.WriteLine($"received!");


                Dictionary<int, byte[]> gotten = new();
                bool waitingForData = true;
                while (waitingForData)
                {
                    var dataMessageBytes = sock.ReceiveFrom(buffer, ref receivingEP);
                    var dataMessageJson = Encoding.ASCII.GetString(buffer, 0, dataMessageBytes);
                    DataMSG dataMessage = JsonSerializer.Deserialize<DataMSG>(dataMessageJson);
                        
                    if (failOn.Contains(dataMessage.Sequence))
                    {
                        failOn[Array.IndexOf(failOn, dataMessage.Sequence)] = -1;
                        continue;
                    }

                    Console.WriteLine($"[seq: {dataMessage.Sequence}] {Encoding.ASCII.GetString(dataMessage.Data)}");
                                            
                    if (!gotten.ContainsKey(dataMessage.Sequence)) 
                    {
                        gotten[dataMessage.Sequence] = dataMessage.Data;
                    }
                    
                    ack.Sequence = dataMessage.Sequence;
                    var ackBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ack));
                    sock.SendTo(ackBytes, ackBytes.Length, SocketFlags.None, remoteEndpoint);
                    
                    if (!dataMessage.More)
                        for (int gottenKey = 0; gottenKey <= dataMessage.Sequence; gottenKey++)
                        {
                            if (!gotten.ContainsKey(gottenKey))
                                break;
                            if (gottenKey == dataMessage.Sequence)
                                waitingForData = false;
                        }
                }

                // TODO: Check if there are more DataMSG messages to be received 
                // receive the message and verify if there are no errors
                //
                

                // TODO: Send back AckMSG for each received DataMSG 


                // TODO: Receive close message
                // receive the message and verify if there are no errors
           
                while (true)
                {

                    int closeRequestServer = sock.ReceiveFrom(buffer, ref receivingEP);
                    
                    var closeRequestString = Encoding.ASCII.GetString(buffer, 0, closeRequestServer);
                    CloseMSG closeRequestMsg = JsonSerializer.Deserialize<CloseMSG>(closeRequestString);
                    if (closeRequestMsg.Type != Messages.CLOSE_REQUEST && closeRequestMsg.ConID == conSettings.ConID)
                    {
                        Console.WriteLine("ERROR:\tGot message that is not a request message or the connection Id was wrong");
                        continue;
                    }
                    Console.WriteLine("Close Reply message is sent");
                    break;
                }


               
                // TODO: confirm close message

                Console.Write("INFO:\tsending Close confirm message to server... ");
                var closeConfirm = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(cls));
                sock.SendTo(closeConfirm, closeConfirm.Length, SocketFlags.None, remoteEndpoint);
                



            }
            catch (Exception err)
            {
                Console.WriteLine("\n Socket Error. Terminating");
                Console.WriteLine("ERROR: " + err.StackTrace);
            }

            Console.WriteLine("Download Complete!");
           
        }
    }
}
