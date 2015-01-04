using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExportDisplay.Winsocket
{
    public class AsynchronousClient
    {
        // The port number for the remote device.
        private const int port = 11000;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent sendDone =
            new ManualResetEvent(false);

        private Socket client;
        public IPAddress ipAddress;
        private IPEndPoint remoteEP;

        public AsynchronousClient()
        {
            // Establish the remote endpoint for the socket.
            // The name of the 
            // remote device is "host.contoso.com".
            ipAddress = Dns.GetHostAddresses("192.168.0.12").SingleOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            client = new Socket(AddressFamily.InterNetwork,
              SocketType.Stream, ProtocolType.Tcp);
        }

        public bool IsConnected
        {
            get { return client.Connected; }
        }

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Connect to the remote endpoint.
                if (!client.Connected)
                {
                    client.BeginConnect(remoteEP,
                                        new AsyncCallback(ConnectCallback), client);
                    connectDone.WaitOne();
                }
                // Send test data to the remote device.
                Send(client, "This is a test ");
                sendDone.WaitOne();



            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void Close()
        {
            // Release the socket.
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
