// FTProtocolClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using PRSLib;
using System.Net;
using System.Net.Sockets;
using System.IO;
using FTLib;
using System.Collections.Generic;

namespace SDBrowser
{
    // implements IProtocolClient
    // uses the FT protocol
    // retrieves an entire directory and represents it as a single text "document"

    class FTProtocolClient : IProtocolClient
    {
        private string prsIP;
        private ushort prsPort;

        public FTProtocolClient(string prsIP, ushort prsPort)
        {
            // save the PRS server's IP address and port
            // will be used later to lookup the port for the FT Server when needed
            this.prsIP = prsIP;
            this.prsPort = prsPort;
        }

        public string GetDocument(string serverIP, string documentName)
        {
            // make sure we have valid parameters
            // serverIP is the FT Server's IP address
            // documentName is the name of a directory on the FT Server
            // both should not be empty
            if (string.IsNullOrWhiteSpace(serverIP) || string.IsNullOrWhiteSpace(documentName))
            {
                throw new Exception("Invalid serverIP or documentName!");
            }

            // contact the PRS and lookup port for "FT Server"
            PRSClient prs = new PRSClient(prsIP, prsPort, "FT Server");
            ushort ftPort = prs.LookupPort();

            // connect to FT server by ipAddr and port
            FTClient ft = new FTClient(serverIP, ftPort);
            ft.Connect();

            try
            {
                List<FTLib.FTClient.File> files = ft.GetDirectory(documentName);

                // combine file contents into one string
                StringBuilder builder = new StringBuilder();
                foreach (FTLib.FTClient.File f in files)
                {
                    builder.AppendLine(f.Name);
                    builder.AppendLine(f.Contents);
                    builder.AppendLine();
                }

                // get the requested directory
                string documentContents = builder.ToString();

                // disconnect from server and close the socket
                ft.Disconnect();

                // return the contents
                return documentContents;
            }
            catch(Exception ex)
            {
                ft.Disconnect();
                throw ex;
            }            
        }

        public void Close()
        {
            // nothing to do here!
            // the FT Protocol does not expect a client to close a session
            // everything is handled in the GetDocument() method
        }        
    }
}
