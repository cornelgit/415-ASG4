// FTClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace FTLib
{
    public class FTClient
    {
        private string ftServerAddress;
        private ushort ftServerPort;
        bool connected;
        Socket clientSocket;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;

        public class File
        {
            public string Name { get; set; }
            public string Contents { get; set; }
        }

        public FTClient(string ftServerAddress, ushort ftServerPort)
        {
            // save server address/port
            this.ftServerAddress = ftServerAddress;
            this.ftServerPort = ftServerPort;

            // initialize to not connected to server
            connected = false;
            clientSocket = null;
            stream = null;
            reader = null;
            writer = null;
        }

        public void Connect()
        {
            if (!connected)
            {
                // create a client socket and connect to the FT Server's IP address and port
                clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ftServerAddress), ftServerPort));

                // establish the network stream, reader and writer
                stream = new NetworkStream(clientSocket);
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                // now connected
                connected = true;
            }
        }

        public void Disconnect()
        {
            if (connected)
            {
                // send exit to FT server
                SendExit();
                
                // close writer, reader and stream
                writer.Close();
                reader.Close();
                stream.Close();

                // disconnect and close socket
                clientSocket.Disconnect(false);
                clientSocket.Close();

                // now disconnected
                connected = false;
            }
        }

        public List<File> GetDirectory(string directoryName)
        {
            // send get to the server for the specified directory and receive files
            if (connected)
            {
                // send get command for the directory
                SendGet(directoryName);

                //SendInvalidMessage();  // uncomment only for testing server error handling

                // receive and process files
                List<File> files = new List<File>();
                File f = null;
                while ((f = ReceiveFile(directoryName)) != null)
                {
                    files.Add(f);
                }

                return files;
            }

            return null;
        }

        #region implementation

        private void SendGet(string directoryName)
        {
            // send get message for the directory
            writer.WriteLine("get");
            writer.WriteLine(directoryName);
            writer.Flush();
            Console.WriteLine("Sent get to server for directory: " + directoryName);
        }

        private void SendExit()
        {
            // send exit message
            writer.WriteLine("exit");
            writer.Flush();
        }

        private void SendInvalidMessage()
        {
            // allows for testing of server's error handling code
            writer.WriteLine("invalid");
            writer.Flush();
        }

        private File ReceiveFile(string directoryName)
        {
            // receive a single file from the server and save it locally in the specified directory

            // expect file name from server
            string fileName = reader.ReadLine();

            // when the server sends "done", then there are no more files!
            if (fileName == "done")
            {
                Console.WriteLine("Received done from server!");
                return null;
            }
            else if (fileName == "error")
            {
                // handle error messages from the server
                string errorMsg = reader.ReadLine();
                Console.WriteLine("Error from server: " + errorMsg);
                return null;
            }

            // received a file name
            // receive file length from server
            int fileLength = int.Parse(reader.ReadLine());
            Console.WriteLine("Receiving file " + fileName + ", " + fileLength.ToString() + " bytes long");

            // receive file contents
            int charsToRead = fileLength;
            string fileContents = "";

            // loop until all of the file contenst are received
            while (charsToRead > 0)
            {
                // receive as many characters from the server as available
                char[] buffer = new char[charsToRead];
                int charsRead = reader.Read(buffer, 0, charsToRead);
                string stringRead = new string(buffer);

                // accumulate bytes read into the contents
                charsToRead -= charsRead;
                fileContents += stringRead;
            }
            Console.WriteLine("Received " + fileLength.ToString() + " bytes: " + fileContents);

            // return file we downloaded
            return new File() { Name = fileName, Contents = fileContents };
        }

        #endregion
    }
}
