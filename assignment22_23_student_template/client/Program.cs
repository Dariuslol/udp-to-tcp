using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Linq;
using UDP_FTP.Models;
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
            var receivingEndpoint = new IPEndPoint(IPAddress.Parse(localIP), 5010);
            var receivingEP = (EndPoint)receivingEndpoint;
    
            var remoteEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 5004);


            HelloMSG h = new HelloMSG();
            h.Type = Messages.HELLO;
            h.To = "MyServer";
            h.From = localIP;

            RequestMSG r = new RequestMSG();
            r.Type = Messages.REQUEST;
            r.To = "MyServer";
            r.From = localIP;
            r.FileName = "test.txt";

            AckMSG ack = new AckMSG();
            ack.Type = Messages.ACK;
            ack.To = "MyServer";
            ack.From = localIP;

            CloseMSG cls = new CloseMSG();
            cls.Type = Messages.CLOSE_CONFIRM;
            cls.To = "MyServer";
            cls.From = localIP;
            


            ConSettings conSettings = new();
            conSettings.From = "MyServer";
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

                switch (VerifyGreeting(helloReply, conSettings)) {
                    case ErrorType.NOERROR:
                        break;
                    default:
                        Console.WriteLine("Gotten response was invalid, quitting");
                        return;
                }
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

                
                switch (VerifyRequest(requestReply, conSettings)) {
                    case ErrorType.NOERROR:
                        break;
                    default:
                        Console.WriteLine("Gotten response was invalid, quitting");
                        Console.WriteLine(requestReplyJson);
                        return;
                }
                Console.WriteLine("received!");


                Dictionary<int, byte[]> gotten = new();
                bool waitingForData = true;
                int lastDataIndex = 0;
                while (waitingForData)
                {
                    var dataMessageBytes = sock.ReceiveFrom(buffer, ref receivingEP);
                    var dataMessageJson = Encoding.ASCII.GetString(buffer, 0, dataMessageBytes);
                    DataMSG dataMessage = JsonSerializer.Deserialize<DataMSG>(dataMessageJson);
                    
                    switch (VerifyData(dataMessage, conSettings)) {
                        case ErrorType.NOERROR:
                            break;
                        default:
                            Console.WriteLine("Gotten data response was invalid, quitting");
                            Console.WriteLine(dataMessageJson);
                            return;
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
                            {
                                Console.WriteLine("INFO:\tServer sent last sequence, but not all data was received");
                                break;
                            }
                            
                            if (gottenKey == dataMessage.Sequence)
                                lastDataIndex = dataMessage.Sequence;
                                waitingForData = false;
                        }
                }
           
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
                    
                    if (VerifyClose(closeRequestMsg, conSettings) != ErrorType.NOERROR)
                    {
                        Console.WriteLine("Got wrong response");
                        Console.WriteLine(closeRequestString);
                        continue;
                    }

                    Console.WriteLine("Close Reply message is sent");
                    break;
                }


                string fileContent = String.Empty;
                foreach (int index in Enumerable.Range(0, gotten.Keys.OrderBy(x => x).Last()))
                {
                    if (!gotten.ContainsKey(index))
                    {
                        Console.WriteLine("ERROR:\tDid not get all the data from the server");
                        break;
                    }
                    
                    fileContent += Encoding.ASCII.GetString(gotten[index]);
                    if (index == lastDataIndex)
                        break;
                }
                    
                Console.WriteLine($"The content of the received file: " + fileContent);
                File.WriteAllText("test.txt", fileContent);

                
                
                Console.Write("INFO:\tsending Close confirm message to server... ");
                var closeConfirm = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(cls));
                sock.SendTo(closeConfirm, closeConfirm.Length, SocketFlags.None, remoteEndpoint);
            }
            catch (Exception err)
            {
                Console.WriteLine("\n Socket Error. Terminating");
                Console.WriteLine("ERROR: " + err.Message + err.StackTrace);
            }

            Console.WriteLine("Download Complete!");
           
        }
        
        public static ErrorType VerifyGreeting( HelloMSG hello, ConSettings C)
        {
            if ( hello.To != C.To || hello.Type != Messages.HELLO_REPLY)
                return ErrorType.BADREQUEST;
            return ErrorType.NOERROR;
        }
        public static ErrorType VerifyRequest( RequestMSG req, ConSettings C)
        {
            if (req.ConID != C.ConID || req.From != C.From || req.To != C.To || req.Type != Messages.REPLY || req.Status != ErrorType.NOERROR)
                return ErrorType.BADREQUEST;
            return ErrorType.NOERROR;
        }
        public static ErrorType VerifyClose( CloseMSG cls, ConSettings C)
        {
            if (cls.ConID != C.ConID || cls.From != C.From || cls.To != C.To || cls.Type != Messages.CLOSE_REQUEST)
                return ErrorType.BADREQUEST;
            return ErrorType.NOERROR;
        }
        
        public static ErrorType VerifyData( DataMSG data, ConSettings C)
        {
            if (data.ConID != C.ConID || data.From != C.From || data.To != C.To || data.Type != Messages.DATA)
                return ErrorType.BADREQUEST;
            return ErrorType.NOERROR;
        }
    }
}
