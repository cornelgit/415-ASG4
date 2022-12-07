// ContentFetcher.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace SDBrowser
{
    class ContentFetcher
    {
        private Dictionary<string, IProtocolClient> protocols;  // protocol name --> protocol client instance

        public ContentFetcher()
        {
            // initially empty protocols dictionary
            protocols = new Dictionary<string, IProtocolClient>();
        }

        public void Close()
        {
            // close each protocol client
            foreach (IProtocolClient client in protocols.Values)
            {
                client.Close();
            }
        }

        public void AddProtocol(string name, IProtocolClient client)
        {
            // save the protocol client under the given name
            protocols.Add(name, client);
        }

        public string Fetch(string address)
        {
            // parse the address
            // Address format:
            //    < type >:< server IP >:< resource >
            //    Where…
            //      < type > is one of “SD” and “FT”
            //      < server IP > is the IP address of the server to contact
            //      < resource > is the name of the resource to request from the server
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new Exception("Empty address!");
            }
            string[] parts = address.Split(':');
            if (parts.Length != 3)
            {
                throw new Exception("Address requires three parts! < type >:< server IP >:< resource >");
            }            

            string type = parts[0];
            string serverIP = parts[1];
            string resource = parts[2];

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(serverIP) || string.IsNullOrEmpty(resource))
            {
                throw new Exception("Address requires value for each part!");
            }

            // retrieve the correct protocol client for the requested protocol
            // watch out for invalid type
            if (!protocols.ContainsKey(type))
            {
                throw new Exception("Invalid protocol type!");
            }

            IProtocolClient client = protocols[type];

            // get the content from the protocol client, using the given IP address and resource name
            string content = client.GetDocument(serverIP, resource);
            
            // return the content
            return content;
        }
    }
}