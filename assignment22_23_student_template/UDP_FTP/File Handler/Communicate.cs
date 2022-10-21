using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UDP_FTP.Error_Handling;
using UDP_FTP.Models;
using static UDP_FTP.Models.Enums;

namespace UDP_FTP.File_Handler
{
    class Communicate
    {
        private const string Server = "MyServer";
        private string Client;
        private int SessionID;
        private Socket socket;
        private IPEndPoint remoteEndpoint;
        private EndPoint remoteEP;
        private ErrorType Status;
        private byte[] buffer;
        byte[] msg;
        private string file;
        ConSettings C;


        public Communicate()
        {
            // TODO: Initializes another instance of the IPEndPoint for the remote host
            remoteEndpoint = new IPEndPoint(IPAddress.Any, 5010);
            remoteEP = (EndPoint)remoteEndpoint;

            // TODO: Specify the buffer size
            buffer = new byte[1000];

            // TODO: Get a random SessionID
            SessionID = new Random().Next();
            Console.WriteLine(SessionID);

            // TODO: Create local IPEndpoints and a Socket to listen 
            //       Keep using port numbers and protocols mention in the assignment description
            //       Associate a socket to the IPEndpoints to start the communication
            var localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5004);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEndpoint);

            C = new ConSettings
            {
                Type = Messages.HELLO,
                From = "127.0.0.1",
                To = "",
                ConID = SessionID,
                Sequence = 0
            };
        }

        public ErrorType StartDownload()
        {
            // TODO: Instantiate and initialize different messages needed for the communication
            // required messages are: HelloMSG, RequestMSG, DataMSG, AckMSG, CloseMSG
            // Set attribute values for each class accordingly 
            HelloMSG GreetBack = new HelloMSG
            {
                ConID = C.ConID,
                From = C.From,
                Type = Messages.HELLO_REPLY,
            };

            RequestMSG req = new RequestMSG
            {
                ConID = C.ConID,
                From = C.From,
                Type = Messages.REPLY,
            };

            DataMSG data = new DataMSG();
            AckMSG ack = new AckMSG();
            CloseMSG cls = new CloseMSG();

            
            // TODO: Start the communication by receiving a HelloMSG message
            // Receive and deserialize HelloMSG message 
            // Verify if there are no errors
            // Type must match one of the ConSettings' types and receiver address must be the server address
            while (true)
            {
                int b = socket.ReceiveFrom(buffer, ref remoteEP);
                var receivedData = Encoding.ASCII.GetString(buffer, 0, b);

                try 
                {
                    var helloMessage = JsonSerializer.Deserialize<HelloMSG>(receivedData);

                    if (helloMessage.To == C.From)
                    {
                        if (helloMessage.Type != C.Type)
                            return ErrorType.BADREQUEST;
                            
                        C.To = helloMessage.From;
                        GreetBack.To = C.To;

                        C.Type = Messages.REQUEST;
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }
            

            // TODO: If no error is found then HelloMSG will be sent back
            var GreetBackJson = JsonSerializer.Serialize(GreetBack);
            var helloReplyMessage = Encoding.ASCII.GetBytes(GreetBackJson);
            socket.SendTo(helloReplyMessage, helloReplyMessage.Length, SocketFlags.None, remoteEP);

            // TODO: Receive the next message
            // Expected message is a download RequestMSG message containing the file name
            // Receive the message and verify if there are no errors
            int requestMessageBytes = socket.ReceiveFrom(buffer, ref remoteEP);
            var requestMessageJson = Encoding.ASCII.GetString(buffer, 0, requestMessageBytes);
            var requestMessage = JsonSerializer.Deserialize<RequestMSG>(requestMessageJson);
            if (requestMessage.Type != Messages.REQUEST && requestMessage.ConID == C.ConID)
            {
                Console.WriteLine("ERROR:\tGot message that is not a request message or the connection Id was wrong");
                return ErrorType.BADREQUEST;
            }
            


            // TODO: Send a RequestMSG of type REPLY message to remoteEndpoint verifying the status
            req.To = C.To;
            req.FileName = requestMessage.FileName;
            req.Status = ErrorType.NOERROR;

            var requestReplyJson = JsonSerializer.Serialize(req);
            var requestReplyBytes = Encoding.ASCII.GetBytes(requestReplyJson);
            socket.SendTo(requestReplyBytes, requestReplyBytes.Length, SocketFlags.None, remoteEP);


            // TODO:  Start sending file data by setting first the socket ReceiveTimeout value
            


            // TODO: Open and read the text-file first
            // Make sure to locate a path on windows and macos platforms



            // TODO: Sliding window with go-back-n implementation
            // Calculate the length of data to be sent
            // Send file-content as DataMSG message as long as there are still values to be sent
            // Consider the WINDOW_SIZE and SEGMENT_SIZE when sending a message  
            // Make sure to address the case if remaining bytes are less than WINDOW_SIZE
            //
            // Suggestion: while there are still bytes left to send,
            // first you send a full window of data
            // second you wait for the acks
            // then you start again.



            // TODO: Receive and verify the acknowledgements (AckMSG) of sent messages
            // Your client implementation should send an AckMSG message for each received DataMSG message   



            // TODO: Print each confirmed sequence in the console
            // receive the message and verify if there are no errors


            // TODO: Send a CloseMSG message to the client for the current session
            // Send close connection request

            // TODO: Receive and verify a CloseMSG message confirmation for the current session
            // Get close connection confirmation
            // Receive the message and verify if there are no errors

            return ErrorType.NOERROR;
        }
    }
}
