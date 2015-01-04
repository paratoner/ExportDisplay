using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExportDisplay.Winsocket
{
    public class AsynchronousServer
    {
        // Thread signal.
        public ManualResetEvent ConnectDone = new ManualResetEvent(false);
        public ManualResetEvent ReceiveDone = new ManualResetEvent(false);

        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // The DNS name of the computer
        // running the listener is "host.contoso.com".
        private IPHostEntry ipHostInfo;
        private IPAddress ipAddress;
        IPEndPoint localEndPoint;
        private Socket server;
        DateTime _lastReceivedTime;
        public AsynchronousServer()
        {
            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList.SingleOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress != null) localEndPoint = new IPEndPoint(ipAddress, 11000);
            // Create a TCP/IP socket.
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }
 
        public void Start()
        {
            Task.Factory.StartNew(StartListening);
        }
        private void StartListening()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                server.Bind(localEndPoint);
                server.Listen(100);

                while (!server.Connected)
                {
                    // Set the event to nonsignaled state.
                    ConnectDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    server.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        server);

                    // Wait until a connection is made before continuing.
                    ConnectDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }
            finally
            {
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            ConnectDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;

            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            server = state.workSocket = handler;
            while (server.Connected)
            {
                ReceiveDone.Reset();
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                     new AsyncCallback(ReadCallback), state);
                ReceiveDone.WaitOne();
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            ReceiveDone.Set();

            _lastReceivedTime = DateTime.Now;
            String content = String.Empty;
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            if (!handler.Connected)
                return;
            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();

                // All the data has been read from the 
                // client. Display it on the console.
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                // Echo the data back to the client.
                //Send(handler, content);
            }
        }

        //private void Send(Socket handler, String data)
        //{
        //    // Convert the string data to byte data using ASCII encoding.
        //    byte[] byteData = Encoding.ASCII.GetBytes(data);

        //    // Begin sending the data to the remote device.
        //    handler.BeginSend(byteData, 0, byteData.Length, 0,
        //        new AsyncCallback(SendCallback), handler);
        //}

        //private void SendCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        // Retrieve the socket from the state object.
        //        Socket handler = (Socket)ar.AsyncState;

        //        // Complete sending the data to the remote device.
        //        int bytesSent = handler.EndSend(ar);
        //        Console.WriteLine("Sent {0} bytes to client.", bytesSent);

        //        handler.Shutdown(SocketShutdown.Both);
        //        handler.Close();

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}


        public void Stop()
        {
            server.Shutdown(SocketShutdown.Both);
            server.Disconnect(true);
        }
    }
}
