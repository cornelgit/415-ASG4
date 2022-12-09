﻿// SDServerProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using PRSLib;

namespace SDServer
{
    class SDServerProgram
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: SDServer -prs <PRS IP address>:<PRS port>");
        }

        static void Main(string[] args)
        {
            // defaults
            ushort SDSERVER_PORT = 40000;
            int CLIENT_BACKLOG = 5;
            string PRS_ADDRESS = "127.0.0.1";
            ushort PRS_PORT = 30000;
            string SERVICE_NAME = "SD Server";

            // process the command line arguments to get the ip address and port number
            try
            {

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-prs")
                    {
                        if (i + 1 < args.Length)
                        {
                            // split serverIP:port
                            string[] parts = args[++i].Split(':');
                            PRS_ADDRESS = parts[0];
                            PRS_PORT = ushort.Parse(parts[1]);
                        }
                        else
                        {
                            throw new Exception("-prs requires a value!");
                        }
                    }
                    else
                    {
                        // error! unexpected cmd line arg
                        throw new Exception("Invalid cmd line arg: " + args[i]);
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error! " + ex.Message);
                return;
            }

            Console.WriteLine("PRS Address: " + PRS_ADDRESS);
            Console.WriteLine("PRS Port: " + PRS_PORT);

            try
            {
                // contact the PRS, request a port for "FT Server" and start keeping it alive
                PRSClient prs = new PRSClient(PRS_ADDRESS, PRS_PORT, SERVICE_NAME);
                SDSERVER_PORT = prs.RequestPort();
                prs.KeepPortAlive();

                // instantiate SD server and start it running
                SDServer sd = new SDServer(SDSERVER_PORT, CLIENT_BACKLOG);
                sd.Start();

                // tell the PRS that it can have it's port back, we don't need it anymore
                prs.ClosePort();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }
    }
}
